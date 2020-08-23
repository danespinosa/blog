using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkWithAzureActiveDirectoryAuth
{
    public class TestDbContext : DbContext
    {
        public TestDbContext([NotNull] DbContextOptions options) : base(options)
        {
            if (Database.IsSqlServer())
            {
                SqlConnection connection = (SqlConnection)Database.GetDbConnection();
                connection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").GetAwaiter().GetResult();
            }
        }

        public DbSet<User> Users { get; set; }
    }
}
