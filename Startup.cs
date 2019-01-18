using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using api.Helpers;
using api.Infrastructure;
using api.Persistent;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            var section = Configuration.GetSection("AppSettings");
            var settings = section.Get<AppSettings>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, Role>(user =>
                {
                    user.Password.RequireDigit = false;
                    user.Password.RequireLowercase = false;
                    user.Password.RequireUppercase = false;
                    user.Password.RequireNonAlphanumeric = false;
                    user.Password.RequiredLength = 4;
                })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication(opts =>
                {
                    opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = settings.Issuer,
                            ValidAudience = settings.Issuer,
                            IssuerSigningKey = JwtTokenHelper.GetSymmetricSecurityKey(settings.Key)
                        };

                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                if (context.Request.Path.Value.StartsWith("/main") &&
                                    context.Request.Query.TryGetValue("token", out StringValues token)
                                )
                                {
                                    context.Token = token;
                                }
                                return Task.CompletedTask;
                            }
                        };
                    });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

            services.AddSignalR();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });

                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(security);
            });

     //       services.AddTransient<NotificationService>();
    //        services.AddSingleton<MessageListener>();

            var serviceProvider = services.BuildServiceProvider();
            var dbContext = serviceProvider.GetService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetService<RoleManager<Role>>();
            var userManager = serviceProvider.GetService<UserManager<User>>();

            Seed(dbContext, userManager, roleManager);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            app.UseHttpsRedirection();
            app.UseStaticFiles();

      //      var listener = app.ApplicationServices.GetService<MessageListener>();
       //     var lifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
       //
       //     lifetime.ApplicationStarted.Register(listener.Start);


            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseSignalR(routes =>
            {
                routes.MapHub<NotifyHub>("/main");
            });

            app.UseMvc();
        }

        public async void Seed(ApplicationDbContext ctx, UserManager<User> manager, RoleManager<Role> roleManager)
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
                        Type = i < 55 ?
                        PlaceType.Normal : PlaceType.Cab
                    });
                }

                await ctx.BathPlaces.AddRangeAsync(list);

                var d1 = new Discount { Name = "Пенсионеры", Value = 300 };
                var d2 = new Discount { Name = "Дети до 7 лет", Value = 0 };
                await ctx.Discount.AddRangeAsync(d1, d2);

                await ctx.SaveChangesAsync();
            }
        }
    }
}
