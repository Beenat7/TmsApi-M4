using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] =
                    "ThisIsASecretKeyForTestingPurposesOnly123456!",
                ["Jwt:Secret"] =
                    "ThisIsASecretKeyForTestingPurposesOnly123456!",
                ["Jwt:Issuer"] = "TmsTestIssuer",
                ["Jwt:Audience"] = "TmsTestAudience",
                ["ConnectionStrings:TmsDatabase"] =
                    "Host=localhost;Database=TmsTestDb;Username=test;Password=test"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TmsDbContext>>();
            services.RemoveAll<TmsDbContext>();

            // services.AddDbContext<TmsDbContext>(options =>
            // {
            //     options.UseInMemoryDatabase("TmsTestDb");
            // });

            var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<TmsDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TmsDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
        });
    }
}