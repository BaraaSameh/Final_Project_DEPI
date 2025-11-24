using DepiFinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Data
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
        public DbSet<FlashSale> FlashSales { get; set; }
        public DbSet<FlashSaleProduct> FlashSaleProducts { get; set; }

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

            // FlashSale Entity Configuration
            modelBuilder.Entity<FlashSale>(entity =>
            {
                entity.HasKey(e => e.FlashSaleID);
                entity.Property(e => e.Title).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(false);

                // Indexes for performance
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);
                entity.HasIndex(e => e.IsActive);
            });

            // FlashSaleProduct Entity Configuration
            modelBuilder.Entity<FlashSaleProduct>(entity =>
            {
                entity.HasKey(e => e.FlashSaleProductID);

                entity.HasOne(e => e.FlashSale)
                      .WithMany(f => f.FlashSaleProducts)
                      .HasForeignKey(e => e.FlashSaleID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.FlashSaleProducts)
                      .HasForeignKey(e => e.ProductID)
                      .OnDelete(DeleteBehavior.Restrict); // Don't delete product if it's in a flash sale

                // Prevent duplicate product in the same flash sale
                entity.HasIndex(e => new { e.FlashSaleID, e.ProductID })
                      .IsUnique();
            });

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
