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
                // -------------------------------------------------------------------
                // 1. FIND AND REMOVE EXISTING CONTEXT REGISTRATIONS
                // We remove the Context itself, not just the options.
                // -------------------------------------------------------------------
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext)).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // -------------------------------------------------------------------
                // 2. FORCE-FEED OPTIONS (THE FIX)
                // Instead of using AddDbContext (which triggers the Program.cs logic),
                // we build the options manually and register them as a Singleton.
                // This bypasses the conflicting "UseSqlServer" configuration entirely.
                // -------------------------------------------------------------------
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase("InMemoryDbForTesting")
                    .Options;

                services.AddSingleton<DbContextOptions<AppDbContext>>(options);

                // 3. Re-register the Context manually so it uses the options above
                services.AddScoped<AppDbContext>();

                // -------------------------------------------------------------------
                // 4. Mock Token Service
                // -------------------------------------------------------------------
                var tokenDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITokenService));
                if (tokenDescriptor != null) services.Remove(tokenDescriptor);

                services.AddSingleton(TokenServiceMock.Object);

                // 5. Create Database
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Database.EnsureDeleted(); // Ensure clean slate
                db.Database.EnsureCreated();
            });
        }
    }
}