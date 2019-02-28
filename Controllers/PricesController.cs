using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Persistent;
using api.Models;
using Microsoft.AspNetCore.SignalR;
using api.Infrastructure;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricesController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        private readonly IHubContext<NotifyHub> hub;

        public PricesController(ApplicationDbContext context, IHubContext<NotifyHub> hub)
        {
            this.context = context;
            this.hub = hub;
        }

        // GET: api/Prices
        [HttpGet]
        public IEnumerable<BathPlacePrice> GetBathPlacePrices()
        {
            return context.BathPlacePrices;
        }

        // PUT: api/Prices/5
        [HttpPut]
        public async Task<IActionResult> PutBathPlacePrice(PriceModel model)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                var price = context.BathPlacePrices.FirstOrDefault(x => x.Id == model.Id);

                price.Price = model.Price;
                context.Entry(price).State = EntityState.Modified;

                try
                {
                    await context.SaveChangesAsync();
                    transaction.Commit();
                    await NotrifyAll();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }

                return NoContent();
            }
        }

        private bool BathPlacePriceExists(int id)
        {
            return context.BathPlacePrices.Any(e => e.Id == id);
        }

        private async Task NotrifyAll()
        {
            await hub.Clients.All.SendAsync("notify", "update-price");
        }
    }
}