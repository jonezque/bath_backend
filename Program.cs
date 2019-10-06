using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using api.Persistent;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace api
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
	            var services = scope.ServiceProvider;
	            var context = services.GetService<ApplicationDbContext>();
	            var user = services.GetService<UserManager<User>>();
	            var role = services.GetService<RoleManager<Role>>();

	            await Seed(context, user, role);
            }
	        await host.RunAsync();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var urls = config.GetSection("AppSettings").GetSection("urls").Value;

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(urls)
                .Build();
        }

		public static async Task Seed(ApplicationDbContext ctx, UserManager<User> manager, RoleManager<Role> roleManager)
        {
            var roleName = "Admin";
            var userName = "admin";
            var password = "admin";

            var adminRole = await roleManager.FindByNameAsync(roleName);
            if (adminRole == null)
            {
                await roleManager.CreateAsync(new Role
                {
                    Name = roleName
                });
            }

            var admin = await manager.FindByNameAsync(userName);

            if (admin == null)
            {
                await manager.CreateAsync(new User
                {
                    UserName = userName
                }, password);

                admin = await manager.FindByNameAsync(userName);
                await manager.AddToRoleAsync(admin, roleName);
            }

            if (!ctx.BathPlaces.Any())
            {
                var p1 = new BathPlacePrice { Price = 320, Type = PlaceType.Normal, Room = RoomType.Men };
                var p2 = new BathPlacePrice { Price = 400, Type = PlaceType.Cab, Room = RoomType.Men };
                var p3 = new BathPlacePrice { Price = 320, Type = PlaceType.Normal, Room = RoomType.Women };
                var p4 = new BathPlacePrice { Price = 400, Type = PlaceType.Cab, Room = RoomType.Women };

                await ctx.BathPlacePrices.AddRangeAsync(p3, p4);

                var list = new List<BathPlace>(78);
                for (int i = 1; i < 79; i++)
                {
                    list.Add(new BathPlace
                    {
                        Name = i.ToString(),
                        Modified = DateTime.Now,
                        Price = i < 55 ? p1 : p2,
                        Room = RoomType.Men,
                        Type = i < 55 ? PlaceType.Normal : PlaceType.Cab
                    });
                }

                await ctx.BathPlaces.AddRangeAsync(list);

                var d1 = new Discount { Name = "Пенсионеры", Value = 300 };
                var d2 = new Discount { Name = "Дети до 7 лет", Value = 0 };
                await ctx.Discount.AddRangeAsync(d1, d2);

                await ctx.SaveChangesAsync();
            }

            if (!ctx.BathPlaces.Any(x => x.Room == RoomType.Women))
            {
                var p1 = await ctx.BathPlacePrices.FirstAsync(x => x.Room == RoomType.Women && x.Type == PlaceType.Normal);
                var p2 = await ctx.BathPlacePrices.FirstAsync(x => x.Room == RoomType.Women && x.Type == PlaceType.Cab);

                var list = new List<BathPlace>(64);
                for (int i = 1; i < 65; i++)
                {
                    list.Add(new BathPlace
                    {
                        Name = i.ToString(),
                        Modified = DateTime.Now,
                        Price = i < 33 ? p1 : p2,
                        Room = RoomType.Women,
                        Type = i < 33 ? PlaceType.Normal : PlaceType.Cab
                    });
                }

                await ctx.BathPlaces.AddRangeAsync(list);

                await ctx.SaveChangesAsync();
            }

            if (!ctx.Products.Any())
            {
                var p1 = new Product { Name = "Тапочки", Price = 200 };
                var p2 = new Product { Name = "Веник", Price = 300 };

                await ctx.Products.AddRangeAsync(p1, p2);

                await ctx.SaveChangesAsync();
            }
        }
    }
}
