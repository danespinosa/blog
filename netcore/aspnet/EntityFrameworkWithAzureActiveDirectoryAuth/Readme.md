# Entity Framework, SQL AAD auth, and CQRS pattern and services Lifetime.

I have a **Razor ASP.NET** app hosted on Azure App Services that uses **Entity Framework** as an ORM to interact with an **Azure SQL DB**.  
That app uses the Command Query Responsibility Segregation **CQRS** pattern to interact with the DB.  
Just a month ago I moved away from using **user/password** auth to **AAD** auth for my Azure SQL Db and also changed my **CQRS** classes to use DI to receive the DbContext using ASP.NET Dependency Injection, that way testing my code would be easier.  
From then on I started a recurrent issue:  
**Login failed for user '<token-identified principal>'. Token is expired.**  

My implementation was [based on this post](https://entityframeworkcore.com/knowledge-base/54187241/ef-core-connection-to-azure-sql-with-managed-identity), but it was a little bit different because my Razor PageModel(s) depended on the Commands and Queries being injected, not the DbContext. This was mainly to be able to make my code testable.  
Here is where I made a mistake, I added my Command and Queries as a **Singleton** services to the services collection instead as **Scoped** which it would be the natural thing to do since the **DbContext** is **Scoped** by default.  
At the time of adding my services as **Singleton** services I didn't really give it a big thought, I just thought that it would work, but I was wrong. It is unnatural to expect that a **Singleton** service could depend in a **Scoped** service since by definition being a **Singleton** service means that its lifetime is the application's lifetime and by having an **Scoped** dependency it would mean that the **Scoped** dependency would be kept for more than the intended time.
In my case, it would mean that at some point the **AAD token** used to connect to **Azure SQL** would expire.  

There's some really good documentation about services lifetime in the [ASP.NET Dependency Injection at the dot.net website](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#scope-validation).

So in a nutshell this is how my code looked like.

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

**launchSettings.json**  
This is very important, specifically the environment variable **ASPNETCORE_ENVIRONMENT** value.
``` json
"EntityFrameworkWithAzureActiveDirectoryAuth": {
    "commandName": "Project",
    "launchBrowser": true,
    "applicationUrl": "https://localhost:5001;http://localhost:5000",
    "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Staging"
    }
```

By default, if you were to run a service with the **ASPNETCORE_ENVIRONMENT** value set to **Development**
 , the application would throw a nice beautiful exception at startup telling you that you are trying to inject an **Scoped**
service into a **Singleton** service.  
*"InvalidOperationException: Cannot consume scoped service 'EntityFrameworkWithAzureActiveDirectoryAuth.TestDbContext' from singleton 'EntityFrameworkWithAzureActiveDirectoryAuth.Query'."*  
But if you are running using **Staging** or **Production** environment then this exception won't throw and then you won't find out the issue until probably it's too late.  

The solution that worked for me:

1.- Enable scope validation when configuring the HostBuilder.  
2.- Update my service to be an Scoped service.  