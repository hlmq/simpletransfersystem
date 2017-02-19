using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SimpleBank.Service.Entities
{
    public class BankUser
    {
        public BankUser()
        {
        }

        public int ID { get; set; }
        [Required]
        [RegularExpression(@"^[A-Za-z0-9\[\]/!$%^&*()\-_+{};:'£@#.?]*$")]
        [Display(Name = "Username")]
        public string AccountNumber { get; set; }
        [Required]
        [Display(Name = "Full Name")]
        public string AccountName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
