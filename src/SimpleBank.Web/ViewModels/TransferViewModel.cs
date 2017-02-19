using SimpleBank.Service.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.Web.ViewModels
{
    public class TransferViewModel
    {
        public BankUser FromUser { get; set; }
        public BankUser ToUser { get; set; }

        public decimal TranferAmount { get; set; }
    }
}
