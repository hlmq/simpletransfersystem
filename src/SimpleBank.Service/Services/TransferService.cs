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
    public class TransferService : ITransferService
    {
        private readonly SimpleBankDbContext _context;

        public TransferService(SimpleBankDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<List<TransferHistory>> GetListTransferHistoryByAccountId(int accountId)
        {
            var lstTransferHistory = await _context.TranferHistories
                                        .Where(x => x.FromUserID == accountId)
                                        .AsNoTracking()
                                        .ToListAsync();
            return lstTransferHistory;
        }

    }
}
