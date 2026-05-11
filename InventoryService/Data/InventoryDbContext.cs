using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data
{
    public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
    {
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<ProcessedOrderEvent> ProcessedOrderEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessedOrderEvent>()
                .HasKey(x => x.OrderId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
