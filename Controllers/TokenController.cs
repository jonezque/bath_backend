using api.Helpers;
using api.Models;
using api.Persistent;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly AppSettings settings;

        private readonly SignInManager<User> signInManager;

        private readonly ApplicationDbContext db;

        public TokenController(
            ApplicationDbContext db,
            IConfiguration configuration,
            SignInManager<User> signInManager)
        {
            this.db = db;
            this.signInManager = signInManager;

            var section = configuration.GetSection("AppSettings");
            settings = section.Get<AppSettings>();
        }

        [HttpPost("Token")]
        public async Task<IActionResult> Token([FromBody]LoginModel login)
        {
            var user = await signInManager.UserManager.FindByNameAsync(login.Username);
            try
            {
                var result = await signInManager.CheckPasswordSignInAsync(user, login.Password, false);
                if (!result.Succeeded)
                {
                    throw new Exception();
                }
            }
            catch
            {
                return BadRequest(new { message = "User not found or incorrect login or password" });
            }

            var identity = await GetIdentity(user);

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                    issuer: settings.Issuer,
                    audience: settings.Issuer,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.AddHours(settings.ExpiryHours),
                    signingCredentials: new SigningCredentials(JwtTokenHelper.GetSymmetricSecurityKey(settings.Key), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(new
            {
                token = encodedJwt
            });
        }

        private async Task<ClaimsIdentity> GetIdentity(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in await signInManager.UserManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }
    }
}
