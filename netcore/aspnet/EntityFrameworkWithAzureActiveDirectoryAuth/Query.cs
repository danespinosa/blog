using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkWithAzureActiveDirectoryAuth
{
    public class Query
    {
        private readonly TestDbContext dbContext;

        public Query(TestDbContext dbContext, ILogger logger = null)
        {
            this.dbContext = dbContext;
        }

        public Task<int> GetRecordCount()
        {
            return dbContext.Users.CountAsync();
        }
    }
}
