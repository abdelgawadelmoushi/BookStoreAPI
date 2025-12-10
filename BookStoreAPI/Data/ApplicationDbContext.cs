using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options) {}
        
        

        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<BookSubImages> BookSubImages { get; set; }
        public DbSet<AuthorBook> Authorbooks { get; set; }
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<BookRating> bookRatings { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Order> orders { get; set; }
        public DbSet<Favourite> favourites { get; set; }


        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);

        //    optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=BookStoreAPI;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntityConfigurations.AuthorBookEntityTypeConfiguration).Assembly);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cart>()
                .Property(c => c.BookPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Book>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Book>()
                .Property(m => m.Discount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Promotion>()
                .Property(p => p.Discount)
                .HasPrecision(18, 2);

        }

    }
}
