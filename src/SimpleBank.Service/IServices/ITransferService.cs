using SimpleBank.Service.Entities;
using SimpleBank.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.Service.IServices
{
    public interface ITransferService
    {
        Task<List<TransferHistory>> GetListTransferHistoryByAccountId(int accountId);
    }
}
