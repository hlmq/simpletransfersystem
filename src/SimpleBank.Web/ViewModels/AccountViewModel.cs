using SimpleBank.Service.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.Web.ViewModels
{
    public class AccountViewModel
    {
        public BankUser User { get; set; }
        [Required]
        public decimal DepositeAmount { get; set; }
        [Required]
        public decimal WithdrawAmount { get; set; }
    }
}
