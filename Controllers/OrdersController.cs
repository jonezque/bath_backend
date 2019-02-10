using api.Infrastructure;
using api.Models;
using api.Persistent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        private readonly UserManager<User> userManager;

        private readonly RoleManager<Role> roleManager;

        private readonly IHubContext<NotifyHub> hub;

        public OrdersController(ApplicationDbContext context,
                                UserManager<User> userManager,
                                RoleManager<Role> roleManager,
                                IHubContext<NotifyHub> hub)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.hub = hub;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery]OrderFilter filter)
        {
            var s = filter.Start.ToUniversalTime().AddHours(3).Date;
            var e = filter.End.ToUniversalTime().AddHours(3).Date;
            return await context.Orders
                            .Where(x => filter.Status == StatusFilter.Both ?
                                true : filter.Status == StatusFilter.Cancel ? x.Canceled : !x.Canceled)
                            .Where(x => filter.Payment == PaymentFilter.Both ?
                                true : filter.Payment == PaymentFilter.Card ? x.Type == PaymentType.Card : x.Type == PaymentType.Cash)
                            .Where(x => filter.Room == RoomFilter.Both ?
                                true : filter.Room == RoomFilter.Male ? x.Room == RoomType.Men : x.Room == RoomType.Women)
                            .Where(x => filter.Date == DateFilter.Day ? x.Modified > s && x.Modified < s.AddDays(1) :
                                x.Modified >= s && x.Modified <= e.AddDays(1))
                            .Include(x => x.BathPlacePositions)
                            .ThenInclude(p => p.BathPlace)
                            .Include(x => x.ProductPositions)
                            .ThenInclude(p => p.Product)
                            .ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // GET: api/Orders
        [HttpGet("getbusyplaces")]
        public async Task<ActionResult<IEnumerable<BathPlacePosition>>> GetBusyPlaces(RoomType room)
        {
            return await GetBusyPositions(room).ToListAsync();
        }

        // POST: api/Orders/createbathplaceorder
        [HttpPost("createbathplaceorder")]
        public async Task<ActionResult> PostOrder(OrderModel model)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                var list = await GetBusyPositions(model.Room).Where(x => model.Places.Select(m => m.Id).Contains(x.BathPlace.Id)).ToListAsync();

                if (list.Any())
                {
                    return BadRequest("place is taken");
                }

                DateTime now = model.Date.ToUniversalTime().AddHours(3);

                var order = new Order
                {
                    BathPlacePositions = model.Places.Select((x) =>
                        new BathPlacePosition
                        {
                            BathPlace = context.BathPlaces
                                .First(b => b.Id == x.Id),
                            Duration = x.Duration,
                            Price = GetPriceByBathId(x.Id),
                            Cost = (GetDiscountById(x.DiscountId) != null ?
                                GetDiscountById(x.DiscountId).Value : GetPriceByBathId(x.Id)) * x.Duration / 60,
                            DiscountName = GetDiscountById(x.DiscountId)?.Name,
                            DiscountValue = GetDiscountById(x.DiscountId)?.Value,
                            Begin = now,
                            End = now.AddMinutes(x.Duration),
                            Status = BathPlaceStatus.Busy,
                        }).ToList(),
                    ProductPositions = model.Products.Select((x) =>
                        new ProductPosition
                        {
                            Count = x.Quantity,
                            Product = context.Products.First(p => p.Id == x.Id),
                            ProductPrice = context.Products.First(p => p.Id == x.Id).Price,
                            TotalPrice = x.Quantity * context.Products.First(p => p.Id == x.Id).Price,
                        }).ToList(),
                    Modified = now,
                    Room = model.Room,
                    Type = model.Type
                };

                order.TotalCost = order.BathPlacePositions.Sum(x => x.Cost) + order.ProductPositions.Sum(x => x.TotalPrice);
                var entry = context.Orders.Add(order);
                await context.SaveChangesAsync();
                transaction.Commit();

                await NotrifyAll(model.Room == RoomType.Men ? "men" : "women");

                return Ok(new { id = entry.Entity.Id });
            }
        }

        // POST: api/Orders/exchangeplaces
        [HttpPost("exchangeplaces")]
        public async Task<ActionResult> ExchangePlaces(ExchangePlaceModel model)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                var fromPlace = await context.BathPlaces
                    .FirstOrDefaultAsync(x => x.Name.Equals(model.From) && x.Room == model.Room);
                var toPlace = await context.BathPlaces
                    .Include(x => x.Price)
                    .FirstOrDefaultAsync(x => x.Name.Equals(model.To) && x.Room == model.Room);

                if (fromPlace.Type != toPlace.Type && fromPlace.Type == PlaceType.Cab)
                {
                    return BadRequest();
                }

                var from = await context.BathPlacePositions
                    .FirstOrDefaultAsync(x => x.BathPlace.Id == fromPlace.Id && x.Status == BathPlaceStatus.Busy);
                var to = await context.BathPlacePositions
                    .FirstOrDefaultAsync(x => x.BathPlace.Id == toPlace.Id && x.Status == BathPlaceStatus.Busy);

                if (from == null || to != null)
                {
                    return BadRequest();
                }

                if (fromPlace.Type == toPlace.Type)
                {
                    from.BathPlace = toPlace;
                } else
                {
                    from.BathPlace = toPlace;
                    from.Price = toPlace.Price.Price;
                    from.Cost = from.Price * from.Duration / 60;

                    var order = await context.Orders
                        .Include(x => x.BathPlacePositions)
                        .Include(x => x.ProductPositions)
                        .FirstOrDefaultAsync(x => x.Id == from.OrderId);

                    order.TotalCost = order.BathPlacePositions.Sum(x => x.Cost) + order.ProductPositions.Sum(x => x.TotalPrice);
                    context.Entry(order).State = EntityState.Modified;
                }

                context.Entry(from).State = EntityState.Modified;
                await context.SaveChangesAsync();
                transaction.Commit();
                await NotrifyAll(model.Room == RoomType.Men ? "men" : "women");

                return Ok();
            }
        }

        // POST: api/Orders/cancelorders
        [HttpPost("cancelorders")]
        public async Task<ActionResult> CancelOrders(CancelOrderModel model)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                var orders = await context.Orders
                    .Include(x => x.ProductPositions)
                    .Include(x => x.BathPlacePositions)
                    .Where(x => model.OrderIds.Contains(x.Id))
                    .ToListAsync();

                var newOrders = new List<Order>();

                orders.ForEach(x => {
                    x.Canceled = true;
                    x.Commentary += model.Reason + $" (заказ №{x.Id})";

                    var notFree = x.BathPlacePositions
                        .Where(pos => !model.BathIds.Any(bathId => bathId == pos.BathPlaceId)).ToList();

                    if (notFree.Count > 0 || x.ProductPositions.Count() > 0)
                    {
                        newOrders.Add(new Order
                        {
                            Commentary = $"новая копия заказа {x.Id} ",
                            Room = x.Room,
                            Type = x.Type,
                            TotalCost = notFree.Sum(pos => pos.Cost) + x.ProductPositions.Sum(pos => pos.TotalPrice),
                            ProductPositions = x.ProductPositions
                                .Select(pos => new ProductPosition
                                {
                                    Count = pos.Count,
                                    ProductId = pos.ProductId,
                                    ProductPrice = pos.ProductPrice,
                                    TotalPrice = pos.TotalPrice,
                                }).ToList(),
                            BathPlacePositions = notFree
                                .Select(pos => new BathPlacePosition
                                {
                                    Price = pos.Price,
                                    DiscountName = pos.DiscountName,
                                    DiscountValue = pos.DiscountValue,
                                    Status = pos.Status,
                                    BathPlaceId = pos.BathPlaceId,
                                    Begin = pos.Begin,
                                    Duration = pos.Duration,
                                    End = pos.End,
                                    Cost = pos.Cost,
                                }).ToList(),
                            Modified = x.Modified
                        });
                    }

                    foreach (var p in x.BathPlacePositions)
                    {
                        p.Status = BathPlaceStatus.Free;
                        context.Entry(p).State = EntityState.Modified;
                    }

                    context.Entry(x).State = EntityState.Modified;
                });

                await context.Orders.AddRangeAsync(newOrders);
                await context.SaveChangesAsync();
                transaction.Commit();

                await NotrifyAll(model.Room == RoomType.Men ? "men" : "women");

                return Ok();
            }
        }

        // POST: api/Orders/addtime
        [HttpPost("addtime")]
        public async Task<ActionResult> AddTime(OrderModel model)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                var list = await GetBusyPositions(model.Room)
                    .Where(x => model.Places.Select(m => m.Id).Contains(x.BathPlace.Id))
                    .ToListAsync();

                if (list.Any(x => x.Status == BathPlaceStatus.Free))
                {
                    return BadRequest();
                }

                list.ForEach(x => {
                    x.Duration += 30;
                    x.End = x.End.Value.AddMinutes(30);
                    x.Cost = (x.DiscountValue ?? x.Price) * x.Duration / 60;
                    context.Entry(x).State = EntityState.Modified;
                });

                var orders = await context.Orders
                    .Where(x => list.Select(p => p.OrderId).Contains(x.Id))
                    .Include(x => x.BathPlacePositions)
                    .Include(x => x.ProductPositions)
                    .ToListAsync();

                orders.ForEach(x => {
                    x.TotalCost = x.BathPlacePositions.Sum(p => p.Cost) + x.ProductPositions.Sum(p => p.TotalPrice);
                    context.Entry(x).State = EntityState.Modified;
                });
                await context.SaveChangesAsync();
                transaction.Commit();

                await NotrifyAll(model.Room == RoomType.Men ? "men" : "women");

                return Ok();
            }
        }

        // POST: api/Orders/freeplaces
        [HttpPost("freeplaces")]
        public async Task<ActionResult> FreePlaces(OrderModel model)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                var list = await GetBusyPositions(model.Room).Where(x => model.Places.Select(m => m.Id).Contains(x.BathPlace.Id)).ToListAsync();

                if (list.Count != model.Places.Count())
                {
                    return BadRequest("place not taken");
                }

                list.ForEach(x =>
                {
                    x.Status = BathPlaceStatus.Free;
                    context.Entry(x).State = EntityState.Modified;
                });

                await context.SaveChangesAsync();
                transaction.Commit();

                await NotrifyAll(model.Room == RoomType.Men ? "men" : "women");

                return Ok(new { ids = list.Select(x => x.Id) });
            }
        }

        private bool OrderExists(int id)
        {
            return context.Orders.Any(e => e.Id == id);
        }

        private decimal GetPriceByBathId(int id)
        {
            return context.BathPlaces.Include(x => x.Price).First(x => x.Id == id).Price.Price;
        }

        private Discount GetDiscountById(int? id)
        {
            return id.HasValue ? context.Discount.First(x => x.Id == id) : null;
        }

        private IQueryable<BathPlacePosition> GetBusyPositions(RoomType room)
        {
            return context.BathPlacePositions
               .Include(x => x.BathPlace)
               .Where(x => x.Status == BathPlaceStatus.Busy && x.BathPlace.Room == room);
        }

        private async Task NotrifyAll(string type)
        {
            await hub.Clients.All.SendAsync("notify", type);
        }
    }
}
