using Microsoft.EntityFrameworkCore;
using SimpleBank.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.Web.Data
{
    //public class SimpleBankDbContext : DbContext
    //{
    //    public SimpleBankDbContext(DbContextOptions<SimpleBankDbContext> options) : base(options)
    //    {
    //    }

    //    public DbSet<BankUser> BankUsers { get; set; }
    //    public DbSet<TransferHistory> TranferHistories { get; set; }

    //    protected override void OnModelCreating(ModelBuilder modelBuilder)
    //    {
    //        modelBuilder.Entity<BankUser>().ToTable("BankUser")
    //                        .Property(p => p.Timestamp).IsConcurrencyToken();

    //        modelBuilder.Entity<TransferHistory>().ToTable("TransferHistory");
    //                        //.Ignore(p => p.ToUserAccountName)
    //                        //.Property(p => p.ToUserAccountName).HasComputedColumnSql("SELECT AccountName FROM BankUser WHERE ID=" + );
    //    }
    //}
}
