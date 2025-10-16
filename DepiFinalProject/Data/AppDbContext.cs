using DepiFinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets لكل كيان
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<Shipping> Shippings { get; set; }
        public DbSet<OrderShipping> OrderShippings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // تكوين العلاقات إذا لزم الأمر (EF Core يتعرف تلقائيًا على معظمها)
            modelBuilder.Entity<OrderShipping>()
                .HasKey(os => new { os.OrderID, os.ShippingID });

            modelBuilder.Entity<OrderShipping>()
                .HasOne(os => os.Order)
                .WithMany(o => o.OrderShippings)
                .HasForeignKey(os => os.OrderID);

            modelBuilder.Entity<OrderShipping>()
                .HasOne(os => os.Shipping)
                .WithMany(s => s.OrderShippings)
                .HasForeignKey(os => os.ShippingID);

            // علاقات أخرى إذا احتجت تخصيص
            base.OnModelCreating(modelBuilder);

        }
    }
}
