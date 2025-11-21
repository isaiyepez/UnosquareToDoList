using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestAPI;
using RestAPI.Interfaces;


namespace ToDoListIntegrationTests
{
    public class IntegrationTestFactory : WebApplicationFactory<Program>
    {
        public Mock<ITokenService> TokenServiceMock { get; } = new Mock<ITokenService>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Remove DbContextOptions<AppDbContext>
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 2. ALSO Remove the non-generic DbContextOptions (Just in case)
                var descriptorNonGeneric = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions));

                if (descriptorNonGeneric != null)
                {
                    services.Remove(descriptorNonGeneric);
                }

                // 3. Add In-Memory Database
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // 4. Replace TokenService
                var tokenDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITokenService));
                if (tokenDescriptor != null) services.Remove(tokenDescriptor);

                services.AddSingleton(TokenServiceMock.Object);

                // 5. Build DB
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
