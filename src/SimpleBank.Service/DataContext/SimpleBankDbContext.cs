using Microsoft.EntityFrameworkCore;
using SimpleBank.Service.Entities;

namespace SimpleBank.Service.Data
{
    public class SimpleBankDbContext : DbContext
    {
        public SimpleBankDbContext(DbContextOptions<SimpleBankDbContext> options) : base(options)
        {
        }

        public DbSet<BankUser> BankUsers { get; set; }
        public DbSet<TransferHistory> TranferHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BankUser>().ToTable("BankUser")
                            .HasIndex(p => p.AccountNumber).IsUnique();
            modelBuilder.Entity<BankUser>().ToTable("BankUser")
                            .Property(p => p.Timestamp)
                            .IsConcurrencyToken();

            modelBuilder.Entity<TransferHistory>().ToTable("TransferHistory");
        }
    }
}
