using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Persistent;
using Microsoft.AspNetCore.SignalR;
using api.Infrastructure;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly IHubContext<NotifyHub> hub;

        public DiscountsController(ApplicationDbContext context, IHubContext<NotifyHub> hub)
        {
            _context = context;
            this.hub = hub;
        }

        // GET: api/Discounts
        [HttpGet]
        public IEnumerable<Discount> GetDiscount()
        {
            return _context.Discount;
        }

        // GET: api/Discounts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiscount([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var discount = await _context.Discount.FindAsync(id);

            if (discount == null)
            {
                return NotFound();
            }

            return Ok(discount);
        }

        // PUT: api/Discounts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDiscount([FromRoute] int id, [FromBody] Discount discount)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != discount.Id)
            {
                return BadRequest();
            }

            _context.Entry(discount).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await NotrifyAll();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DiscountExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Discounts
        [HttpPost]
        public async Task<IActionResult> PostDiscount([FromBody] Discount discount)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Discount.Add(discount);
            await _context.SaveChangesAsync();
            await NotrifyAll();

            return CreatedAtAction("GetDiscount", new { id = discount.Id }, discount);
        }

        // DELETE: api/Discounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscount([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var discount = await _context.Discount.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }

            _context.Discount.Remove(discount);
            await _context.SaveChangesAsync();
            await NotrifyAll();

            return Ok(discount);
        }

        private bool DiscountExists(int id)
        {
            return _context.Discount.Any(e => e.Id == id);
        }

        private async Task NotrifyAll()
        {
            await hub.Clients.All.SendAsync("notify", "update-discount");
        }
    }
}