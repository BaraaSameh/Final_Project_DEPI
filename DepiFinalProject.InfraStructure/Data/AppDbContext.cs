using DepiFinalProject.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.InfraStructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets for each entity
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
        public DbSet<Wishlist> Wishlists { get; set; }

        public DbSet<ReturnSettings> ReturnSettings { get; set; }

        // RefreshTokens DbSet "Seif"
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<OTP> OtpEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relationships and keys configuration
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

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ReturnSettings>()
                .Property(r => r.AllowedReturnStatuses)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            modelBuilder.Entity<ReturnSettings>()
                .Property(r => r.AllowedReturnReasons)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            // Relationship between Wishlist and Product
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.ProductID)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite Key to prevent duplicate: User + Product (Wishlist)
            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new { w.UserID, w.ProductID })
                .IsUnique();

         

            // RefreshToken Entity Configuration "Seif"
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.Token).IsUnique();
            });

            // Configure the Product entity's relationship with User
            modelBuilder.Entity<Product>()
              .HasOne(p => p.user)
              .WithMany(u => u.Products)
              .HasForeignKey(p => p.userid)
              .OnDelete(DeleteBehavior.Restrict);
           

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // ProductImage Entity Configuration
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Additional configurations if needed
            base.OnModelCreating(modelBuilder);
        }
    }
}
