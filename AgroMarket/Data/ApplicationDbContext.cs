using AgroMarket.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroMarket.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Retailer> Retailers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }




        // OnModelCreating method to configure relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // Enforce a unique constraint on the Email column
            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Retailer>()
                .HasIndex(e => e.Email)
                .IsUnique();


            // Configure many-to-many relationship between Products and Categories
            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategory)
                .HasForeignKey(pc => pc.ProductId);

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategory)
                .HasForeignKey(pc => pc.CategoryId);

            //Configure relationships for other entities

           //modelBuilder.Entity<Retailer>()
           //    .HasOne(r => r.Customer)
           //    .WithOne(u => u.Retailer)
           //    .HasForeignKey<Retailer>(r => r.CustomerID);

           modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Order)
                .HasForeignKey(o => o.CustomerID);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Review)
                .HasForeignKey(r => r.ProductID);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.Review)
                .HasForeignKey(r => r.CustomerID);

            modelBuilder.Entity<Order>()
           .Property(o => o.TotalAmount)
           .HasColumnType("decimal(18,2)") // Specify the SQL column type
           .HasPrecision(18, 2); // Specify precision and scale

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)") // Specify the SQL column type
                .HasPrecision(18, 2); // Specify precision and scale

            modelBuilder.Entity<Cart>()
            .HasKey(c => new { c.CustomerID, c.ProductID }); // Configure composite key
            modelBuilder.Entity<Cart>()
            .HasOne(c => c.Customer)
            .WithMany(c=>c.Cart) // Assuming a customer can have many carts
            .HasForeignKey(c => c.CustomerID)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Define delete behavior

            modelBuilder.Entity<Cart>()
            .HasOne(c => c.Product)
            .WithMany(c=>c.Cart) // Assuming a product can be in many carts
            .HasForeignKey(c => c.ProductID)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Define delete behavior
        }


    }
}
