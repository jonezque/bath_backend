using System.Threading.Tasks;
using api.Helpers;
using api.Infrastructure;
using api.Persistent;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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

			services.AddControllers();

			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

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
							    context.Request.Query.TryGetValue("token", out var token)
							)
								context.Token = token;
							return Task.CompletedTask;
						}
					};
				});

			services.AddAuthorization(auth =>
			{
				auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
					.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
					.RequireAuthenticatedUser().Build());
			});

			services.AddSignalR();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo {Title = "My API", Version = "v1"});
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Description =
						"JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					In = ParameterLocation.Header
				});
				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}
						},
						new string[] { }
					}
				});
			});

			//       services.AddTransient<NotificationService>();
			//        services.AddSingleton<MessageListener>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else
				app.UseHsts();
			app.UseRouting();
			app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			//      var listener = app.ApplicationServices.GetService<MessageListener>();
			//     var lifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
			//
			//     lifetime.ApplicationStarted.Register(listener.Start);


			app.UseAuthentication();
			app.UseAuthorization();
			app.UseSwagger();
			app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHub<NotifyHub>("/main");
			});
		}
	}
}