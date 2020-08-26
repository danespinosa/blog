using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkWithAzureActiveDirectoryAuth
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
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
