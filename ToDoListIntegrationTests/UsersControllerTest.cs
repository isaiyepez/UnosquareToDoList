using Entities;
using Entities.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using ToDoListIntegrationTests;

public class UsersControllerTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFactory _factory;

    public UsersControllerTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldCreateUser_WhenDtoIsValid()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            DisplayName = "Test User",
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Users", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var returnedUser = await response.Content.ReadFromJsonAsync<UserDto>();
        returnedUser.Should().NotBeNull();
        returnedUser.Email.Should().Be(registerDto.Email);
        returnedUser.Token.Should().Be("LOL-thisIsNotARealToken-butItLooksLegit-SoDontUseIt-Seriously1234!");
    }

    [Fact]
    public async Task Patch_ShouldUpdateUser_WhenJsonIsValid()
    {
        // Arrange - Create user first
        var registerDto = new RegisterDto
        {
            DisplayName = "Test2 User",
            Email = "test2@example.com",
            Password = "Password123!"
        };

        var registrationResponse = await _client.PostAsJsonAsync("/api/Users", registerDto);
        registrationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var returnedUser = await registrationResponse.Content.ReadFromJsonAsync<UserDto>();
        returnedUser.Should().NotBeNull();

        // Login to get authentication token
        var loginDto = new LoginDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/Users/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loggedInUser = await loginResponse.Content.ReadFromJsonAsync<UserDto>();
        loggedInUser.Should().NotBeNull();
        loggedInUser.Token.Should().NotBeNullOrEmpty();

        // Set authentication header for subsequent requests
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loggedInUser.Token);

        // Act - Prepare and send PATCH request
        var patchDoc = new JsonPatchDocument<UserDto>();
        patchDoc.Replace(u => u.DisplayName, "Updated Name");

        var patchContent = new StringContent(
            JsonConvert.SerializeObject(patchDoc),
            Encoding.UTF8,
            "application/json-patch+json");

        var patchResponse = await _client.PatchAsync($"/api/Users/{returnedUser.Id}", patchContent);

        // Assert - Verify PATCH response
        patchResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update persisted in the database
        loginResponse = await _client.PostAsJsonAsync("/api/Users/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        loggedInUser = await loginResponse.Content.ReadFromJsonAsync<UserDto>();

        loggedInUser.Should().NotBeNull();
        loggedInUser.DisplayName.Should().Be("Updated Name");
        loggedInUser.Id.Should().Be(returnedUser.Id); // Ensure same user
    }

    [Fact]
    public async Task Login_ShouldReturnUser_WhenCredentialsAreCorrect()
    {
        // Arrange: We must register a user first so they exist in the In-Memory DB
        var registerDto = new RegisterDto
        {
            DisplayName = "Login User",
            Email = "login@example.com",
            Password = "MySecretPassword"
        };
        await _client.PostAsJsonAsync("/api/Users", registerDto);

        var loginDto = new LoginDto
        {
            Email = "login@example.com",
            Password = "MySecretPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Users/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var returnedUser = await response.Content.ReadFromJsonAsync<UserDto>();
        returnedUser.Should().NotBeNull();
        returnedUser.Email.Should().Be(loginDto.Email);
        returnedUser.Token.Should().NotBeNullOrEmpty("Authentication token was missing from the Login response");

    }

    [Fact]
    public async Task Login_ShouldFail_WhenPasswordIsWrong()
    {
        // Arrange: Register user
        var registerDto = new RegisterDto
        {
            DisplayName = "Wrong Pass User",
            Email = "wrongpass@example.com",
            Password = "CorrectPassword"
        };
        await _client.PostAsJsonAsync("/api/Users", registerDto);

        var loginDto = new LoginDto
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Users/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

}