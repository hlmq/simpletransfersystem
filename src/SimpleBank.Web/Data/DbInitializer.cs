using SimpleBank.Service.Data;
using SimpleBank.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.Web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SimpleBankDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any users.
            if (context.BankUsers.Any())
            {
                return;   // DB has been seeded
            }

            var users = new BankUser[]
            {
            new BankUser{ AccountName="edward", AccountNumber="account1", Balance=1000, Password="test123", CreatedDate=DateTime.Now },
            new BankUser{ AccountName="will", AccountNumber="account2", Balance=50, Password="test123", CreatedDate=DateTime.Now },
            };
            foreach (BankUser s in users)
            {
                context.BankUsers.Add(s);
            }
            
            context.SaveChanges();
        }
    }
}
