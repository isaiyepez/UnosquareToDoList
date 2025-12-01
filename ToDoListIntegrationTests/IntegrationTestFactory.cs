using BusinessLogic.Contracts;
using BusinessLogic.Services; // Assuming ToDoTaskService is here
using Data;
using Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

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

                // Remove existing ITokenService registration
                var tokenDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITokenService));
                if (tokenDescriptor != null) services.Remove(tokenDescriptor);

                // Add mocked ITokenService
                services.AddSingleton(TokenServiceMock.Object);

                TokenServiceMock.Setup(x => x.CreateToken(It.IsAny<User>()))
                    .Returns("LOL-thisIsNotARealToken-butItLooksLegit-SoDontUseIt-Seriously1234!");

                // Register your real ToDoTaskService implementation for IToDoTaskService
                var toDoTaskServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IToDoTaskService));
                if (toDoTaskServiceDescriptor != null) services.Remove(toDoTaskServiceDescriptor);

                services.AddScoped<IToDoTaskService, ToDoTaskService>();

                // Add Test Authentication scheme
                services
                   .AddAuthentication(options =>
                   {
                       options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                       options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                   })
                   .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                       TestAuthHandler.AuthenticationScheme, options => { });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        }
    }
}
