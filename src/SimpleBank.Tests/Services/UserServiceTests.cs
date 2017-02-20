using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SimpleBank.Service.Data;
using SimpleBank.Service.Services;
using SimpleBank.Service.Entities;
using System.Threading;

namespace SimpleBank.Tests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public void Deposit_with_negative_amount()
        {
            // Arrange
            var fakeTimestamp = BitConverter.GetBytes(DateTime.Now.Ticks);
            var options = new DbContextOptionsBuilder<SimpleBankDbContext>()
                .UseInMemoryDatabase(databaseName: "Deposit_with_negative_amount")
                .Options;
            using (var context = new SimpleBankDbContext(options))
            {
                var service = new UserService(context);

                var task = Task.Run(async () =>
                {
                    return await service.CreateNewUser(new BankUser
                    {
                        AccountNumber = "test",
                        AccountName = "tester tests",
                        Balance = 10000,
                        Password = "123456",
                        CreatedDate = DateTime.Now,
                        Timestamp = fakeTimestamp
                    });
                });

                var isCreatedSuccess = task.Result;
                Assert.True(isCreatedSuccess == 1);
                Assert.True(context.BankUsers.Single(x => x.AccountNumber == "test") != null);
            }

            // Act
            using (var context = new SimpleBankDbContext(options))
            {
                var service = new UserService(context);

                var task = Task.Run(async () =>
                {
                    return await service.Deposit(
                        context.BankUsers.Single(x => x.AccountNumber == "test").ID,
                        -100,
                        fakeTimestamp
                        );
                });

                // Assert
                var result = task.Result;
                Assert.True(result.ErrorList.Any());
            }
        }

        [Fact]
        public void Withdraw_with_negative_amount()
        {
            // Arrange
            var fakeTimestamp = BitConverter.GetBytes(DateTime.Now.Ticks);
            var options = new DbContextOptionsBuilder<SimpleBankDbContext>()
                .UseInMemoryDatabase(databaseName: "Withdraw_with_negative_amount")
                .Options;
            using (var context = new SimpleBankDbContext(options))
            {
                var service = new UserService(context);

                var task = Task.Run(async () =>
                {
                    return await service.CreateNewUser(new BankUser
                    {
                        AccountNumber = "test",
                        AccountName = "tester tests",
                        Balance = 10000,
                        Password = "123456",
                        CreatedDate = DateTime.Now,
                        Timestamp = fakeTimestamp
                    });
                });

                var isCreatedSuccess = task.Result;
                Assert.True(isCreatedSuccess == 1);
                Assert.True(context.BankUsers.Single(x => x.AccountNumber == "test") != null);
            }

            // Act
            using (var context = new SimpleBankDbContext(options))
            {
                var service = new UserService(context);

                var task = Task.Run(async () =>
                {
                    return await service.Withdraw(
                        context.BankUsers.Single(x => x.AccountNumber == "test").ID,
                        -100,
                        fakeTimestamp
                        );
                });

                // Assert
                var result = task.Result;
                Assert.True(result.ErrorList.Any());
            }
        }

        [Fact]
        public void Withdraw_with_exceeding_amount()
        {
            // Arrange
            var fakeTimestamp = BitConverter.GetBytes(DateTime.Now.Ticks);
            var options = new DbContextOptionsBuilder<SimpleBankDbContext>()
                .UseInMemoryDatabase(databaseName: "Withdraw_with_exceeding_amount")
                .Options;
            using (var context = new SimpleBankDbContext(options))
            {
                var service = new UserService(context);

                var task = Task.Run(async () =>
                {
                    return await service.CreateNewUser(new BankUser
                    {
                        AccountNumber = "test",
                        AccountName = "tester tests",
                        Balance = 10000,
                        Password = "123456",
                        CreatedDate = DateTime.Now,
                        Timestamp = fakeTimestamp
                    });
                });

                var isCreatedSuccess = task.Result;
                Assert.True(isCreatedSuccess == 1);
                Assert.True(context.BankUsers.Single(x => x.AccountNumber == "test") != null);
            }

            // Act
            using (var context = new SimpleBankDbContext(options))
            {
                var service = new UserService(context);

                var task = Task.Run(async () =>
                {
                    return await service.Withdraw(
                        context.BankUsers.Single(x => x.AccountNumber == "test").ID,
                        20000,
                        fakeTimestamp
                        );
                });

                // Assert
                var result = task.Result;
                Assert.True(result.ErrorList.Any());
            }
        }
        
        [Fact]
        public void Transfer_with_exceeding_amount()
        {
            // Arrange
            var user1Timestamp = BitConverter.GetBytes(DateTime.Now.Ticks);
            var user2Timestamp = BitConverter.GetBytes(DateTime.Now.AddMilliseconds(1).Ticks);
            var options = new DbContextOptionsBuilder<SimpleBankDbContext>()
                .UseInMemoryDatabase(databaseName: "Transfer_with_exceeding_amount")
                .Options;
            using (var context = new SimpleBankDbContext(options))
            {
                var service = new UserService(context);

                var user1CreatedOk = Task.Run(async () =>
                {
                    return await service.CreateNewUser(new BankUser
                    {
                        AccountNumber = "user1",
                        AccountName = "User 1",
                        Balance = 10000,
                        Password = "123456",
                        CreatedDate = DateTime.Now,
                        Timestamp = user1Timestamp
                    });
                });

                var isUser1CreatedSuccess = user1CreatedOk.Result;
                Assert.True(isUser1CreatedSuccess == 1);
                Assert.True(context.BankUsers.Single(x => x.AccountNumber == "user1") != null);

                var user2CreatedOk = Task.Run(async () =>
                {
                    return await service.CreateNewUser(new BankUser
                    {
                        AccountNumber = "user2",
                        AccountName = "User 2",
                        Balance = 100,
                        Password = "123456",
                        CreatedDate = DateTime.Now,
                        Timestamp = user2Timestamp
                    });
                });

                var isUser2CreatedSuccess = user1CreatedOk.Result;
                Assert.True(isUser1CreatedSuccess == 1);
                Assert.True(context.BankUsers.Single(x => x.AccountNumber == "user2") != null);
            }

            // Act
            using (var context = new SimpleBankDbContext(options))
            {
                var service = new UserService(context);

                var task = Task.Run(async () =>
                {
                    return await service.Transfer(
                        context.BankUsers.Single(x => x.AccountNumber == "user1").ID,
                        context.BankUsers.Single(x => x.AccountNumber == "user2").ID,
                        20000,
                        user1Timestamp,
                        user2Timestamp
                        );
                });

                // Assert
                var result = task.Result;
                Assert.True(result.ErrorList.Any());
            }
        }

        [Fact]
        public void Add_writes_to_database()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<SimpleBankDbContext>()
                .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
                .Options;

            // Act
            using (var context = new SimpleBankDbContext(options))
            {
                var service = new UserService(context);

                var task = Task.Run(async () =>
                {
                    return await service.CreateNewUser(new BankUser
                    {
                        AccountNumber = "test",
                        AccountName = "tester tests",
                        Balance = 10000,
                        Password = "123456",
                        CreatedDate = DateTime.Now
                    });
                });

                var isCreatedSuccess = task.Result;
                Assert.True(isCreatedSuccess == 1);
            }

            // Assert
            using (var context = new SimpleBankDbContext(options))
            {
                Assert.Equal(1, context.BankUsers.Count());
                Assert.Equal("tester tests", context.BankUsers.Single().AccountName);
            }
        }
    }
}
