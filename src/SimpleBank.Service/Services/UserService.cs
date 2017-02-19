using Microsoft.EntityFrameworkCore;
using SimpleBank.Service.Data;
using SimpleBank.Service.Entities;
using SimpleBank.Service.IServices;
using SimpleBank.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.Service.Services
{
    public class UserService : IUserService
    {
        private readonly SimpleBankDbContext _context;

        public UserService(SimpleBankDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<BankUser> GetUserByAccountNoAndPassword(string accountNo, string password)
        {
            return await _context.BankUsers.SingleOrDefaultAsync(m => m.AccountNumber == accountNo
                                        && m.Password == password);
        }

        public async Task<List<BankUser>> GetListAllUser()
        {
            var lstAccount = await _context.BankUsers
                                        .AsNoTracking()
                                        .ToListAsync();
            return lstAccount;
        }

        public async Task<int> CreateNewUser(BankUser user)
        {
            _context.Add(user);
            var result = await _context.SaveChangesAsync();
            return result;
        }

        public async Task<BankUser> GetAccountById(int id)
        {
            var user = await _context.BankUsers.SingleOrDefaultAsync(m => m.ID == id);
            return user;
        }

        public async Task<BankUserUpdateModel> Deposit(int id, decimal amount, byte[] accountTimestamp)
        {
            var updateStateModel = new BankUserUpdateModel();

            if (amount <= 0)
            {
                updateStateModel.ErrorList.Add("DepositeAmount", "The amount must be larger than 0.");
                updateStateModel.ErrorList.Add(string.Empty, "Unable to save changes.");
                return updateStateModel;
            }

            var accountToUpdate = await this.GetAccountById(id);
            if (accountToUpdate == null)
            {
                updateStateModel.ErrorList.Add(string.Empty, "Unable to save changes. The account was deleted by another user.");
                return updateStateModel;
            }

            // deposit the amount into Account
            accountToUpdate.Balance = accountToUpdate.Balance + amount;

            // set the Timestamp value for the retrieved Account. This Timestamp value will be compared to detech concurrence issue
            _context.Entry(accountToUpdate).Property("Timestamp").OriginalValue = accountTimestamp;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                updateStateModel.ErrorList.Add(string.Empty, "The record you attempted to edit was modified by another user after you got the original value.");

                var exceptionEntry = ex.Entries.Single();
                // Using a NoTracking query means we get the entity but it is not tracked by the context
                // and will not be merged with existing entities in the context.
                var modifiedEntityInDatabase = await _context.BankUsers
                .AsNoTracking()
                .SingleAsync(d => d.ID == ((BankUser)exceptionEntry.Entity).ID);

                var latestEntityInDb = _context.Entry(modifiedEntityInDatabase);

                // get the latest Balance amount of the Account from Database
                var latestBalance = (Decimal)latestEntityInDb.Property("Balance").CurrentValue;
                if (latestBalance != accountToUpdate.Balance)
                {
                    updateStateModel.ErrorList.Add("User.Balance", $"Current value: {latestBalance:c}");
                }

                // re-update the Timestamp value into User.Timestamp. The deposit process can work for the next Submit
                updateStateModel.EntityTimestamp = (byte[])latestEntityInDb.Property("Timestamp").CurrentValue;
            }

            return updateStateModel;
        }
        
        public async Task<BankUser> GetAccountByIdWithNoTracking(int id)
        {
            var user = await _context.BankUsers
                    .AsNoTracking()
                    .SingleAsync(d => d.ID == id);

            return user;
        }


        public async Task<BankUserUpdateModel> Withdraw(int id, decimal amount, byte[] accountTimestamp)
        {
            var updateStateModel = new BankUserUpdateModel();

            if (amount <= 0)
            {
                updateStateModel.ErrorList.Add("WithdrawAmount", "The amount must be larger than 0.");
                updateStateModel.ErrorList.Add(string.Empty, "Unable to save changes.");
                return updateStateModel;
            }

            var accountToUpdate = await this.GetAccountById(id);
            if (accountToUpdate == null)
            {
                updateStateModel.ErrorList.Add(string.Empty, "Unable to save changes. The account was deleted by another user.");
                return updateStateModel;
            }

            if (accountToUpdate.Balance - amount < 0)
            {
                updateStateModel.ErrorList.Add("WithdrawAmount", "The withdraw amount must be larger or equals the balance amount.");
                updateStateModel.ErrorList.Add(string.Empty, "Unable to save changes.");
                return updateStateModel;
            }

            // deposit the amount into Account
            accountToUpdate.Balance = accountToUpdate.Balance - amount;

            // set the Timestamp value for the retrieved Account. This Timestamp value will be compared to detech concurrence issue
            _context.Entry(accountToUpdate).Property("Timestamp").OriginalValue = accountTimestamp;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                updateStateModel.ErrorList.Add(string.Empty, "The record you attempted to edit was modified by another user after you got the original value.");

                var exceptionEntry = ex.Entries.Single();
                // Using a NoTracking query means we get the entity but it is not tracked by the context
                // and will not be merged with existing entities in the context.
                var modifiedEntityInDatabase = await _context.BankUsers
                .AsNoTracking()
                .SingleAsync(d => d.ID == ((BankUser)exceptionEntry.Entity).ID);

                var latestEntityInDb = _context.Entry(modifiedEntityInDatabase);

                // get the latest Balance amount of the Account from Database
                var latestBalance = (Decimal)latestEntityInDb.Property("Balance").CurrentValue;
                if (latestBalance != accountToUpdate.Balance)
                {
                    updateStateModel.ErrorList.Add("User.Balance", $"Current value: {latestBalance:c}");
                }

                // re-update the Timestamp value into User.Timestamp. The deposit process can work for the next Submit
                updateStateModel.EntityTimestamp = (byte[])latestEntityInDb.Property("Timestamp").CurrentValue;
            }

            return updateStateModel;
        }

        public async Task<BankUserUpdateModel> Transfer(
            int fromUserId, 
            int toUserId, 
            decimal amount, 
            byte[] fromAccountTimestamp,
            byte[] toAccountTimestamp)
        {
            var updateStateModel = new BankUserUpdateModel();

            if (amount <= 0)
            {
                updateStateModel.ErrorList.Add("TranferAmount", "The amount must be larger than 0.");
                updateStateModel.ErrorList.Add(string.Empty, "Unable to save changes.");
                return updateStateModel;
            }

            var accountToTransfer = await this.GetAccountById(fromUserId);
            if (accountToTransfer == null)
            {
                updateStateModel.ErrorList.Add(string.Empty, "Unable to save changes. The transfer account was deleted by another user.");
                return updateStateModel;
            }

            if (accountToTransfer.Balance - amount < 0)
            {
                updateStateModel.ErrorList.Add("TranferAmount", "The withdraw amount must be larger or equals the balance amount.");
                updateStateModel.ErrorList.Add(string.Empty, "Unable to save changes.");
                return updateStateModel;
            }

            var accountToReceive = await this.GetAccountById(toUserId);
            if (accountToReceive == null)
            {
                updateStateModel.ErrorList.Add(string.Empty, "Unable to save changes. The received account was deleted by another user.");
                return updateStateModel;
            }

            try
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    // create TransferHistory record
                    var transferHistoryRecord = new TransferHistory
                    {
                        FromUserID = accountToTransfer.ID,
                        ToUserID = accountToReceive.ID,
                        CreatedDate = DateTime.Now,
                        Amount = amount
                    };
                    _context.TranferHistories.Add(transferHistoryRecord);

                    // transfer the amount into Account
                    accountToTransfer.Balance = accountToTransfer.Balance - amount;
                    accountToReceive.Balance = accountToReceive.Balance + amount;

                    // set the Timestamp value for the retrieved Account. This Timestamp value will be compared to detech concurrence issue
                    _context.Entry(accountToTransfer).Property("Timestamp").OriginalValue = fromAccountTimestamp;
                    _context.Entry(accountToReceive).Property("Timestamp").OriginalValue = toAccountTimestamp;

                    await _context.SaveChangesAsync();

                    transaction.Commit();
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                updateStateModel.ErrorList.Add(string.Empty, "The transfer is failed.");

                foreach (var exceptionEntry in ex.Entries)
                {
                    var userId = ((BankUser)exceptionEntry.Entity).ID;
                    // Using a NoTracking query means we get the entity but it is not tracked by the context
                    // and will not be merged with existing entities in the context.
                    var modifiedEntityInDatabase = await _context.BankUsers
                        .AsNoTracking()
                        .SingleAsync(d => d.ID == userId);
                    var latestEntityInDb = _context.Entry(modifiedEntityInDatabase);

                    // get the latest Balance amount of the Account from Database
                    var latestBalance = (Decimal)latestEntityInDb.Property("Balance").CurrentValue;

                    if (userId == accountToTransfer.ID && latestBalance != accountToTransfer.Balance)
                    {
                        updateStateModel.ErrorList.Add("FromUser.Balance", $"Current value: {latestBalance:c}");
                    }
                    if (userId == accountToReceive.ID && latestBalance != accountToReceive.Balance)
                    {
                        updateStateModel.ErrorList.Add("ToUser.Balance", $"Current value: {latestBalance:c}");
                    }
                }
            }

            return updateStateModel;
        }
    }
}
