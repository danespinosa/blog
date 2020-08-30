using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkWithAzureActiveDirectoryAuth
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
            // This is commented since the LocalDb does not take AAD tokens.
            // In the real life scenario, where the DB was an Azure SQL server this code would get the AAD token and set it in the connection each HTTP request.
            //if (Database.IsSqlServer())
            //{
            //    SqlConnection connection = (SqlConnection)Database.GetDbConnection();
            //    connection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").GetAwaiter().GetResult();
            //}
        }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }
    }
}
