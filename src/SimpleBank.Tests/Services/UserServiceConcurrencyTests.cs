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
    public class UserServiceConcurrencyTests
    {
        public UserServiceConcurrencyTests()
        {
        }

        /*
         * User 1 will do 2 transfers to User 2 and User 3 asynchronously
         * Context 1: User 1 will transfer $900 to User 2 (Balance=100). New balance of User 2 will be 1000 if transfer is success.
         * Context 2: User 1 will transfer $1000 to User 3 (Balance=1000). New balance of User 3 will be 2000 if transfer is success.
         */
        [Fact]
        public async Task User1_transfer_to_both_users_at_same_time_method_1()
        {
            var contextCreator = new TestingDbContextCreator("User1_transfer_to_both_users_at_same_time_method_1");
            var context = contextCreator.CreateSimpleBankDbContextForTesting();
            // Arrange the data for the test
            decimal user1Balance = 10000; decimal user2Balance = 100; decimal user3Balance = 1000;
            await this.CreateUserTask("user1", "12345", user1Balance, context);
            await this.CreateUserTask("user2", "12345", user2Balance, context);
            await this.CreateUserTask("user3", "12345", user3Balance, context);

            var context1 = contextCreator.GetFreshDbContext();
            var context2 = contextCreator.GetFreshDbContext();

            var user1InContext1 = context1.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user1");
            var user2InContext1 = context1.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user2");
            var user1InContext2 = context2.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user1");
            var user3InContext2 = context2.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user3");

            // Act
            // initialize the transfer in 2 different dbContext
            var transferInContext1 = new UserService(context1).Transfer(
                    user1InContext1.ID, user2InContext1.ID,
                    900,
                    user1InContext1.Timestamp, user2InContext1.Timestamp);

            var transferInContext2 = new UserService(context2).Transfer(
                    user1InContext2.ID, user3InContext2.ID,
                    1000,
                    user1InContext2.Timestamp, user3InContext2.Timestamp);

            // exec 2 transfer in parallel
            var allTasks = await Task.WhenAll(transferInContext1, transferInContext2);

            // Assert
            // assert there is existing ErrorList in allTasks
            Assert.True(allTasks.Any(x => x.ErrorList.Count > 0));
            // create new dbContext to query latest data in database
            using (var assertContext = contextCreator.GetFreshDbContext())
            {
                var updatedUser1 = assertContext.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user1");
                var updatedUser2 = assertContext.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user2");
                var updatedUser3 = assertContext.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "user3");

                // assert the balance of user1 is 9100 (if transfer1 is success) OR 9000 (if transfer2 is success)
                Assert.True(updatedUser1.Balance == 9100 || updatedUser1.Balance == 9000);
                // assert the balance of user2 is 1000 AND balance of user3 is 1000 if the transferInContext1 is success
                // OR assert the balance of user2 is 100 AND balance of user3 is 2000 if the transferInContext2 is success
                Assert.True((updatedUser2.Balance == 1000 && updatedUser3.Balance == 1000)
                    || (updatedUser2.Balance == 100 && updatedUser3.Balance == 2000));
            }
        }

        [Fact]
        public async Task User1_transfer_to_both_users_at_same_time_method_2()
        {
            var contextCreator = new TestingDbContextCreator("User1_transfer_to_both_users_at_same_time_method_2");
            var context = contextCreator.CreateSimpleBankDbContextForTesting();
            // Arrange the data for the test
            decimal userABalance = 10000; decimal userBBalance = 100; decimal userCBalance = 1000;
            await this.CreateUserTask("userA", "6789", userABalance, context);
            await this.CreateUserTask("userB", "6789", userBBalance, context);
            await this.CreateUserTask("userC", "6789", userCBalance, context);

            var context1 = contextCreator.GetFreshDbContext();
            var context2 = contextCreator.GetFreshDbContext();

            // create context1 data
            var userAInContext1 = context1.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "userA");
            var userBInContext1 = context1.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "userB");
            // create context2 data
            var userAInContext2 = context2.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "userA");
            var userCInContext2 = context2.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "userC");

            // Act
            // exec 2 transfer in parallel
            ConcurrentDictionary<string, Task<BankUserUpdateModel>> resultInParallel = new ConcurrentDictionary<string, Task<BankUserUpdateModel>>();
            Parallel.Invoke(
                () => 
                {
                    var transfer = new UserService(context1).Transfer(
                        userAInContext1.ID, userBInContext1.ID,
                        900,
                        userAInContext1.Timestamp, userBInContext1.Timestamp);
                    resultInParallel.TryAdd("transferInContext1", transfer);
                },
                () =>
                {
                    var transfer = new UserService(context2).Transfer(
                        userAInContext2.ID, userCInContext2.ID,
                        1000,
                        userAInContext2.Timestamp, userCInContext2.Timestamp);
                    resultInParallel.TryAdd("transferInContext2", transfer);
                });

            // Assert
            // assert there is existing ErrorList in resultInParallel
            Assert.True(resultInParallel.Any(x => x.Value.Result.ErrorList.Count > 0)); 
            // create new dbContext to query latest data in database
            using (var assertContext = contextCreator.GetFreshDbContext())
            {
                var updatedUserA = assertContext.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "userA");
                var updatedUserB = assertContext.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "userB");
                var updatedUserC = assertContext.BankUsers.AsNoTracking().SingleOrDefault(x => x.AccountNumber == "userC");

                // assert the balance of userA is 9100 (if transfer1 is success) OR 9000 (if transfer2 is success)
                Assert.True(updatedUserA.Balance == 9100 || updatedUserA.Balance == 9000);
                // assert the balance of userB is 1000 AND balance of userC is 1000 if the transferInContext1 is success
                // OR assert the balance of userB is 100 AND balance of userC is 2000 if the transferInContext2 is success
                Assert.True((updatedUserB.Balance == 1000 && updatedUserC.Balance == 1000)
                    || (updatedUserB.Balance == 100 && updatedUserC.Balance == 2000));
            }
        }

        #region private methods
        private async Task<int> CreateUserTask(string accountName, string password, decimal balance, SimpleBankDbContext context)
        {
            return await new UserService(context).CreateNewUser(
                    new BankUser
                    {
                        AccountNumber = accountName,
                        AccountName = accountName + password,
                        Balance = balance,
                        Password = password,
                        CreatedDate = DateTime.Now
                    });
        }
        #endregion

        [Fact]
        public void Save_user_successfully_in_testing_sql_server_returns_Timestamp_data()
        {
            // Arrange
            var context = new TestingDbContextCreator("Save_user_successfully_in_testing_sql_server_returns_Timestamp_data")
                .CreateSimpleBankDbContextForTesting();
            var service = new UserService(context);
            
            // must ensure db created before calling transaction
            using (var transaction = context.Database.BeginTransaction())
            {
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
                Assert.True(context.BankUsers.Single(x => x.AccountNumber == "test") != null);

                transaction.Rollback();
            }
        }
    }
}
