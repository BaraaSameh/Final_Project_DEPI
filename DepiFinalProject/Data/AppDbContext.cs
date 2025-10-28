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
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<FlashSale> FlashSales { get; set; }
        public DbSet<FlashSaleProduct> FlashSaleProducts { get; set; }

       


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

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // تكوين العلاقة بين Wishlist و Product
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.ProductID)
                .OnDelete(DeleteBehavior.Cascade);

            // مفتاح مركب (Composite Key) لمنع التكرار: مستخدم + منتج واحد بس
            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new { w.UserID, w.ProductID })
                .IsUnique();

            // FlashSale

            modelBuilder.Entity<FlashSale>(entity =>
            {
                entity.HasKey(e => e.FlashSaleID);
                entity.Property(e => e.Title).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(false);

                // فهرس للأداء
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);
                entity.HasIndex(e => e.IsActive);
            });

            // FlashSaleProduct
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
                      .OnDelete(DeleteBehavior.Restrict); // لا نحذف المنتج لو في عرض

                // منع تكرار منتج في نفس العرض
                entity.HasIndex(e => new { e.FlashSaleID, e.ProductID })
                      .IsUnique();
            });



            // علاقات أخرى إذا احتجت تخصيص
            base.OnModelCreating(modelBuilder);


        }
    }
}
