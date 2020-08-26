using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkWithAzureActiveDirectoryAuth
{
    public class Query
    {
        private readonly TestDbContext dbContext;

        public Query(TestDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<int> GetRecordCount()
        {
            return dbContext.Users.CountAsync();
        }
    }
}
