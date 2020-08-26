# Entity Framework, SQL AAD auth, and CQRS pattern and services Lifetime.

I have a **Razor ASP.NET** app hosted on Azure App Services that uses **Entity Framework** as an ORM to interact with an **Azure SQL DB**.  
That app uses the Command Query Responsibility Segregation **CQRS** pattern to interact with the DB.  
Just a month ago I moved away from using **user/password** auth to **AAD** auth for my Azure SQL Db and also changed my **CQRS** classes to use DI to receive the DbContext using ASP.NET Dependency Injection, that way testing my code would be easier.  
From then on I started a recurrent issue:  
**Login failed for user '<token-identified principal>'. Token is expired.**  

My implementation was [based on this post](https://entityframeworkcore.com/knowledge-base/54187241/ef-core-connection-to-azure-sql-with-managed-identity), but it was a little bit different because my Razor PageModel(s) depended on the Commands and Queries being injected, not the DbContext. This was mainly to be able to make my code testable.  
There is where I made a mistake, I added my Command and Queries as a **Singleton** service to the services collection instead as **Scoped** which it would be the natural thing to do since the **DbContext** is **Scoped** by default.  
At the time of adding my service as a **Singleton** I didn't really give it a big thought, I just thought that it would work, but I was wrong. It is unnatural to expect that a **Singleton** service could depend in a **Scoped** service since by definition being a **Singleton** service means that your scope is the application's life time and by having an **Scoped** dependency it would mean that 


So in a nutshell this is how my startup code looked like below.

**Startup.cs**
``` CSharp
public void ConfigureServices(IServiceCollection services)
{
    string connectionString = Configuration.GetConnectionString("Test");
    services.AddDbContext<TestDbContext>(builder => builder.UseSqlServer(connectionString));
    services.AddSingleton<Query>();
}
```

**Query.cs**

``` CSharp
public class Query
{
    private readonly TestDbContext dbContext;

    public Query(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<int> GetRecordCount()
    {
        return dbContext.Accounts.CountAsync();
    }
}
```

**TestDbContext.cs**
``` CSharp
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

    public DbSet<User> Accounts { get; set; }
}
```

By adding my query as a Singleton service, my web application was creating the Query object at the time of startup and it would be injected to my Razor Pages.