using api.Models;
using api.Persistent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly RoleManager<Role> roleManager;

        private readonly SignInManager<User> signInManager;

        private readonly ApplicationDbContext db;

        public AccountController(ApplicationDbContext db,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager)
        {
            this.db = db;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }

        [HttpGet]
        public async Task<UserModel> Get()
        {
            var user = await signInManager.UserManager.GetUserAsync(User);
            var roles = await signInManager.UserManager.GetRolesAsync(user);
            return new UserModel
            {
                Name = user.UserName,
                Roles = roles
            };
        }

        [HttpPost]
        public async Task<bool> RoleMatch(RolesModel model)
        {
            var user = await signInManager.UserManager.GetUserAsync(User);
            var roles = await signInManager.UserManager.GetRolesAsync(user);
            return model.Roles.ToList().Intersect(roles).Any();
        }
    }
}
