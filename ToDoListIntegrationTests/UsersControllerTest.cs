using Entities;
using Entities.DTOs;
using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using ToDoListIntegrationTests;

public class UsersControllerTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFactory _factory;

    public UsersControllerTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Setup default mock behavior for Token Service
        _factory.TokenServiceMock.Setup(x => x.CreateToken(It.IsAny<User>()))
            .Returns("fake-jwt-token");
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedUser = await response.Content.ReadFromJsonAsync<UserDto>();
        returnedUser.Should().NotBeNull();
        returnedUser.Email.Should().Be(registerDto.Email);
        returnedUser.Token.Should().Be("fake-jwt-token");
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