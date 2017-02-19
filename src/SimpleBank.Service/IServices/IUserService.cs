using SimpleBank.Service.Entities;
using SimpleBank.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.Service.IServices
{
    public interface IUserService
    {
        Task<BankUser> GetUserByAccountNoAndPassword(string accountNo, string password);

        Task<List<BankUser>> GetListAllUser();

        Task<int> CreateNewUser(BankUser user);

        Task<BankUser> GetAccountById(int id);

        Task<BankUserUpdateModel> Deposit(int id, decimal amount, byte[] accountTimestamp);

        Task<BankUser> GetAccountByIdWithNoTracking(int id);

        Task<BankUserUpdateModel> Withdraw(int id, decimal amount, byte[] accountTimestamp);

        Task<BankUserUpdateModel> Transfer(
            int fromUserId,
            int toUserId,
            decimal amount,
            byte[] fromAccountTimestamp,
            byte[] toAccountTimestamp);
    }
}
