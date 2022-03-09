using Microsoft.EntityFrameworkCore;
using OrderBookSystemClient.Data.Entities;
using OrderBookSystemClient.Models;


namespace OrderBookSystemClient.Data
{
    public class DBContext : DbContext
    {
        public DBContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(@"Server=DESKTOP-M6LRN8N\SQLEXPRESS;Database=BookSystemDB;Trusted_Connection=True;MultipleActiveResultSets=true");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDetail>().HasKey(a => new { a.order_id, a.user_id, a.booksusers_id});

        }

        public DbSet<Book> Books { get; set; }
        public DbSet<BookUser> BooksUsers { get; set; }
        public DbSet<BookType> BookTypes { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailManagement> EmailManagements { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<OrderDetail> OrdersDetails { get; set; }

        public DbSet<EmailType> EmailTypes { get; set; }

    }
}
