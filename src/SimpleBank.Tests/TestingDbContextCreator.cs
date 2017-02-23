using SimpleBank.Service.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SimpleBank.Tests
{
    public class TestingDbContextCreator : IDisposable
    {
        private SimpleBankDbContext _context;
        private DbContextOptionsBuilder<SimpleBankDbContext> _dbContextBuilder;
        private readonly Guid genGUID = Guid.NewGuid();

        public TestingDbContextCreator(string unitTestName)
        {
            this._dbContextBuilder = new DbContextOptionsBuilder<SimpleBankDbContext>()
                .UseSqlServer(this.GenerateTestingDatabaseName(unitTestName));
        }

        public SimpleBankDbContext CreateSimpleBankDbContextForTesting()
        {
            this._context = new SimpleBankDbContext(_dbContextBuilder.Options);

            // ensure db created
            _context.Database.EnsureCreated();

            return _context;
        }

        public SimpleBankDbContext GetFreshDbContext()
        {
            return new SimpleBankDbContext(_dbContextBuilder.Options);
        }

        private string GenerateTestingDatabaseName(string unitTestName)
        {
            return $"Server=(localdb)\\mssqllocaldb;Database={unitTestName}_{genGUID};Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
