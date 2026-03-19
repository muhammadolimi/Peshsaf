using Microsoft.EntityFrameworkCore;

namespace SomonStore.Models
{
    public class ShopDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!; 
        public DbSet<Payment> Payments { get; set; } = null!; 
        public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!; 
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Promotion> Promotions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql("Host=localhost;Port=5432;Database=SomonStore_db;Username=postgres;Password=2005");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopDbContext).Assembly);
        }

    }
}