using SimpleBank.Service.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [RegularExpression(@"^[A-Za-z0-9\[\]/!$%^&*()\-_+{};:'£@#.?]*$")]
        public string AccountNumber { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        //public BankUser User { get; set; }
    }
}
