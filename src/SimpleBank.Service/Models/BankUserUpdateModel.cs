using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.Service.Models
{
    public class BankUserUpdateModel
    {
        public BankUserUpdateModel()
        {
            ErrorList = new Dictionary<string, string>();
            EntityTimestamp = null;
        }

        public Dictionary<string, string> ErrorList { get; set; }
        public byte[] EntityTimestamp { get; set; }
    }
}
