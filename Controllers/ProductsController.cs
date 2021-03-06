﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Infrastructure;
using api.Persistent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Admin,Manager")]
	public class ProductsController : ControllerBase
	{
		private readonly ApplicationDbContext context;

		private readonly IHubContext<NotifyHub> hub;

		private readonly RoleManager<Role> roleManager;

		private readonly UserManager<User> userManager;

		public ProductsController(ApplicationDbContext context, UserManager<User> userManager,
			RoleManager<Role> roleManager, IHubContext<NotifyHub> hub)
		{
			this.context = context;
			this.userManager = userManager;
			this.roleManager = roleManager;
			this.hub = hub;
		}

		// GET: api/Products
		[HttpGet("getproducts")]
		public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
		{
			return await context.Products.ToListAsync();
		}

		[HttpGet("getbathplaces")]
		public async Task<ActionResult<IEnumerable<BathPlace>>> GetBathPlaces(RoomType room)
		{
			return await context.BathPlaces.Where(x => x.Room == room).Include(x => x.Price).ToListAsync();
		}

		// GET: api/Products/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Product>> GetProduct(int id)
		{
			var product = await context.Products.FindAsync(id);

			if (product == null) return NotFound();

			return product;
		}

		// PUT: api/Products/5
		[HttpPut("{id}")]
		public async Task<IActionResult> PutProduct(int id, Product product)
		{
			if (id != product.Id) return BadRequest();

			var date = DateTime.Now;
			var user = await userManager.GetUserAsync(User);

			product.Modified = date;

			context.Entry(product).State = EntityState.Modified;

			try
			{
				await context.SaveChangesAsync();
				await NotrifyAll();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ProductExists(id))
					return NotFound();
				throw;
			}

			return NoContent();
		}

		// POST: api/Products
		[HttpPost]
		public async Task<ActionResult<Product>> PostProduct(Product product)
		{
			var date = DateTime.Now;
			var user = await userManager.GetUserAsync(User);

			product.Modified = date;
			context.Products.Add(product);
			await context.SaveChangesAsync();
			await NotrifyAll();

			return CreatedAtAction("GetProduct", new {id = product.Id}, product);
		}

		// DELETE: api/Products/5
		[HttpDelete("{id}")]
		public async Task<ActionResult<Product>> DeleteProduct(int id)
		{
			var product = await context.Products.FindAsync(id);
			if (product == null) return NotFound();

			context.Products.Remove(product);
			await context.SaveChangesAsync();
			await NotrifyAll();

			return product;
		}

		private bool ProductExists(int id)
		{
			return context.Products.Any(e => e.Id == id);
		}

		private async Task NotrifyAll()
		{
			await hub.Clients.All.SendAsync("notify", "update-product");
		}
	}
}