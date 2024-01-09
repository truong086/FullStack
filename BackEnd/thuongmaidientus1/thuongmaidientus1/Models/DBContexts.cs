using Microsoft.EntityFrameworkCore;
using System.Data;

namespace thuongmaidientus1.Models
{
    public class DBContexts : DbContext
    {
        public DBContexts(DbContextOptions options) : base(options) { }

        #region DBSet
        public DbSet<Account> accounts { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<Product> products { get; set; }
        public DbSet<Order> orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<ProductCategory> productCategories { get; set; }
        public DbSet<Roles> roles { get; set; }
        public DbSet<Tokens> tokens { get; set; }
        public DbSet<Shop> shops { get; set; }
        public DbSet<ShopVanchuyen> shopVanchuyens { get; set; }
        public DbSet<Vanchuyen> vanchuyens { get; set; }
        public DbSet<ProductImage> productimages { get; set; }
        public DbSet<Xulydonhang> xulydonhangs { get; set; }
        public DbSet<Payment> payments { get; set; }
        public DbSet<Merchant> merchants { get; set; }
        public DbSet<PaymentTransaction> paymentTransactions { get; set; }
        public DbSet<PaymentDescription> paymentDescriptions { get; set; }
        public DbSet<PaymentNotification> paymentNotifications { get; set; }
        public DbSet<PaymentSignature> paymentSignatures { get; set; }

        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("User Id=sa;Password=1234;Server=LAPTOP-D5OKMO3H\\SQLEXPRESS;Database=thuongmaidientus1")
                         .EnableSensitiveDataLogging(); // Bật chế độ ghi log chi tiết
        }
    }
}
