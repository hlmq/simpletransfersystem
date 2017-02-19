using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SimpleBank.Service.Data;
using SimpleBank.Service.Services;
using SimpleBank.Service.Entities;

namespace SimpleBank.Tests
{
    public class Class1
    {
        public Class1()
        {
        }

        [Fact]
        public void PassingTest()
        {
            Assert.Equal(4, Add(2, 2));
        }
        
        int Add(int x, int y)
        {
            return x + y;
        }
    }
    
}
