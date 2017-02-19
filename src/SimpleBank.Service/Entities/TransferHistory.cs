using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.Service.Entities
{
    public class TransferHistory
    {
        public TransferHistory()
        {
        }

        public int ID { get; set; }

        [Required]
        public int FromUserID { get; set; }
        [Required]
        public int ToUserID { get; set; }

        //[NotMapped]
        //public string ToUserAccountName { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }
    }
}
