using Microsoft.EntityFrameworkCore;

namespace Northwind.Services.EntityFramework.Entities
{
    public class NorthwindContext : DbContext
    {
        public NorthwindContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = default!;

        public DbSet<Employee> Employees { get; set; } = default!;

        public DbSet<Order> Orders { get; set; } = default!;

        public DbSet<OrderDetail> OrderDetails { get; set; } = default!;

        public DbSet<Product> Products { get; set; } = default!;

        public DbSet<Shipper> Shippers { get; set; } = default!;

        public DbSet<Supplier> Suppliers { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            _ = modelBuilder?.Entity<Order>().Property(od => od.ShippedDate)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Order>().Property(od => od.ShipRegion)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Supplier>().Property(od => od.Region)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Supplier>().Property(s => s.ContactName)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Supplier>().Property(s => s.ContactTitle)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Supplier>().Property(s => s.Address)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Supplier>().Property(s => s.City)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Supplier>().Property(s => s.PostalCode)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Supplier>().Property(s => s.Country)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Supplier>().Property(s => s.Phone)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Supplier>().Property(s => s.Fax)
                .IsRequired(false);
            _ = modelBuilder?.Entity<Supplier>().Property(s => s.HomePage)
                .IsRequired(false);
        }
    }
}
