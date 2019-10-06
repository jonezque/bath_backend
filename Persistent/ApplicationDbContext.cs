using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Persistent
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<BathPlace> BathPlaces { get; set; }
        public DbSet<BathPlacePrice> BathPlacePrices { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductPosition> ProductPositions { get; set; }
        public DbSet<BathPlacePosition> BathPlacePositions { get; set; }
        public DbSet<Discount> Discount { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Master> Masters { get; set; }
        public DbSet<ServicePosition> ServicePositions { get; set; }
    }
}
