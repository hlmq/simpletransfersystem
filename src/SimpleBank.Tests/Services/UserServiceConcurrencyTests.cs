using Microsoft.EntityFrameworkCore;
using SimpleBank.Service.Data;
using SimpleBank.Service.Entities;
using SimpleBank.Service.Models;
using SimpleBank.Service.Services;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleBank.Tests.Services
{
    public class UserServiceConcurrencyTests : IDisposable
    {
        private SimpleBankDbContext _context;
        private UserService _service;
        private DbContextOptionsBuilder<SimpleBankDbContext> _dbContextBuilder;
        public UserServiceConcurrencyTests()
        {
            _dbContextBuilder = new DbContextOptionsBuilder<SimpleBankDbContext>()
                .UseSqlServer($"Server=(localdb)\\mssqllocaldb;Database=SimpleBank_Testing_{Guid.NewGuid()};Trusted_Connection=True;MultipleActiveResultSets=true");

            _context = new SimpleBankDbContext(_dbContextBuilder.Options);

            // ensure db created
            _context.Database.EnsureCreated();
            _service = new UserService(_context);
        }
        public void Dispose()
        {
            _service = null;
            _context.Database.EnsureDeleted();
        }

        /*
         * Test case: User 1 will do transfer to User 2 (in browser 1) and User 3 (in browser 2)
         * Step 0: prepare data User 1 (Balance = 10000) | User 2 (Balance = 100) | User 3 (Balance = 1000)
         * Step 1: open browser 1 with User 1 and User 2
         * Step 2: open browser 2 with User 1 and User 3
         * Step 3: transfering in browser 1, User 1 transfer to User 2 the amount = 1000 and submit successful. Browser 1 returns to Account page
         * Step 4: transfering in browser 2, User 1 transfer to User 2 the amount = 1000 * 2 and submit.
         *         The transfer is invalid with message for new User1.Balance = (10000 - amount) "Current value: $9000". Browser 2 remains page
         */
        [Fact]
        public async Task User1_transfer_to_both_users_at_same_time()
        {
            // Arrange
            decimal amount = 1000;
            decimal user1Balance = 10000; decimal user2Balance = 100; decimal user3Balance = 1000;
            await this.CreateUserTask("user1", "12345", user1Balance);
            await this.CreateUserTask("user2", "12345", user2Balance);
            await this.CreateUserTask("user3", "12345", user3Balance);

            var user1 = _context.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user1");
            var user2 = _context.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user2");
            var user3 = _context.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user3");

            // Act
            var successTransfer = _service.Transfer(
                    user1.ID, user2.ID,
                    amount,
                    user1.Timestamp, user2.Timestamp);

            // assume successTransfer will execute Ok first, then failedTransfer with old Timestamp value will be executed and failed
            await Task.Delay(100);

            var failedTransfer = _service.Transfer(
                    user1.ID, user3.ID,
                    amount * 2,
                    user1.Timestamp, user3.Timestamp);

            await Task.WhenAll(successTransfer, failedTransfer);

            var transfer1Result = successTransfer.Result;
            var transfer2Result = failedTransfer.Result;
            
            // Assert
            Assert.True(transfer1Result.ErrorList.Count == 0);
            Assert.True(transfer2Result.ErrorList.Count > 0);
            Assert.True(transfer2Result.ErrorList.Any(x => x.Value.Equals($"Current value: {(user1Balance - amount):c}")));
            // create new dbContext to query latest data in database
            using (var freshContext = new SimpleBankDbContext(_dbContextBuilder.Options))
            {
                var freshUser1 = _context.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user1");
                var freshUser2 = _context.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user2");
                var freshUser3 = _context.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user3");
                Assert.Equal(freshUser1.Balance, 9000);
                Assert.Equal(freshUser2.Balance, 1100);
                Assert.Equal(freshUser3.Balance, 1000);
            }

        }

        #region private methods
        private Task CreateUserTask(string accountName, string password, decimal balance)
        {
            var task = Task.Run(async () =>
            {
                return await _service.CreateNewUser(
                    new BankUser
                    {
                        AccountNumber = accountName,
                        AccountName = accountName + password,
                        Balance = balance,
                        Password = password,
                        CreatedDate = DateTime.Now
                    });
            });

            return task;
        }
        #endregion

        [Fact]
        public void Save_user_successfully_in_testing_sql_server_returns_Timestamp_data()
        {
            // must ensure db created before calling transaction
            using (var transaction = _context.Database.BeginTransaction())
            {
                // Arrange
                var service = new UserService(_context);

                // Act
                var task = Task.Run(async () =>
                {
                    return await service.CreateNewUser(
                        new BankUser
                        {
                            AccountNumber = "test",
                            AccountName = "tester tests",
                            Balance = 10000,
                            Password = "123456",
                            CreatedDate = DateTime.Now
                        });
                });

                // Assert
                var isCreatedSuccess = task.Result;
                Assert.True(isCreatedSuccess == 1);
                Assert.True(_context.BankUsers.Single(x => x.AccountNumber == "test") != null);

                transaction.Rollback();
            }
        }
    }
}
