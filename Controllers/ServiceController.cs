using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Persistent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        private readonly UserManager<User> userManager;

        private readonly RoleManager<Role> roleManager;

        public ServiceController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpPost("createservice")]
        public async Task<ActionResult> PostService(Service service)
        {
            return Ok();
        }
    }
}