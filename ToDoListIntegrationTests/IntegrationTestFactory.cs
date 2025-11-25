using BusinessLogic.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Data;

namespace ToDoListIntegrationTests
{
    public class IntegrationTestFactory : WebApplicationFactory<Program>
    {
        public Mock<ITokenService> TokenServiceMock { get; } = new Mock<ITokenService>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {

                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext)).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase("InMemoryDbForTesting")
                    .Options;

                services.AddSingleton<DbContextOptions<AppDbContext>>(options);

                services.AddScoped<AppDbContext>();

                var tokenDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITokenService));
                if (tokenDescriptor != null) services.Remove(tokenDescriptor);

                services.AddSingleton(TokenServiceMock.Object);

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Database.EnsureDeleted(); 
                db.Database.EnsureCreated();
            });
        }
    }
}