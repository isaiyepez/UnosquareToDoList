using Entities;
using Entities.DTOs;
using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ToDoListIntegrationTests;

public class ToDoTasksControllerTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFactory _factory;

    public ToDoTasksControllerTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostToDoTask_ShouldReturnCreated_WhenDtoIsValid()
    {
        // Arrange
        var user = await CreateAuthenticatedUserAsync();

        var taskDto = new ToDoTaskDto
        {
            Title = "Finish Integration Tests",
            UserId = user.Id,
            IsDone = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ToDoTasks", taskDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTask = await response.Content.ReadFromJsonAsync<ToDoTaskDto>();
        createdTask.Should().NotBeNull();
        createdTask.Title.Should().Be(taskDto.Title);
        createdTask.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTasksListAsync_ShouldReturnTasks_WhenUserIdIsValid()
    {
        // Arrange
        var user = await CreateAuthenticatedUserAsync();

        // Seed a task for this user
        var taskDto = new ToDoTaskDto { Title = "Task 1", UserId = user.Id };
        await _client.PostAsJsonAsync("/api/ToDoTasks", taskDto);

        // Act
        var response = await _client.GetAsync($"/api/ToDoTasks?userId={user.Id}&skip=0&take=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tasks = await response.Content.ReadFromJsonAsync<IEnumerable<ToDoTaskDto>>();
        tasks.Should().NotBeNullOrEmpty();
        tasks.Should().Contain(t => t.Title == "Task 1");
    }

    [Fact]
    public async Task GetTasksListAsync_ShouldReturnBadRequest_WhenUserIdIsMissing()
    {
        // Arrange
        await CreateAuthenticatedUserAsync();

        // Act
        var response = await _client.GetAsync("/api/ToDoTasks"); // No Query Params

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("UserId query parameter must be provided");
    }

    [Fact]
    public async Task GetToDoTaskByIdAsync_ShouldReturnTask_WhenIdExists()
    {
        // Arrange
        var user = await CreateAuthenticatedUserAsync();

        // Create a task first
        var createResponse = await _client.PostAsJsonAsync("/api/ToDoTasks",
            new ToDoTaskDto { Title = "Find Me", UserId = user.Id });
        var createdTask = await createResponse.Content.ReadFromJsonAsync<ToDoTaskDto>();

        createdTask.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/ToDoTasks/{createdTask.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedTask = await response.Content.ReadFromJsonAsync<ToDoTaskDto>();
        fetchedTask.Should().NotBeNull();
        fetchedTask.Id.Should().Be(createdTask.Id);
        fetchedTask.Title.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetToDoTaskByIdAsync_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        // Arrange
        await CreateAuthenticatedUserAsync();
        int nonExistentId = 99999;

        // Act
        var response = await _client.GetAsync($"/api/ToDoTasks/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateToDoTask_ShouldReturnNoContent_WhenUpdateIsValid()
    {
        // Arrange
        var user = await CreateAuthenticatedUserAsync();

        // Create original task
        var createResponse = await _client.PostAsJsonAsync("/api/ToDoTasks",
            new ToDoTaskDto { Title = "Original Title", UserId = user.Id });
        var createdTask = await createResponse.Content.ReadFromJsonAsync<ToDoTaskDto>();

       createdTask.Should().NotBeNull();

        // Prepare update
        createdTask.Title = "Updated Title";
        createdTask.IsDone = true;

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ToDoTasks/{createdTask.Id}", createdTask);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update in DB
        var getResponse = await _client.GetAsync($"/api/ToDoTasks/{createdTask.Id}");
        var updatedTask = await getResponse.Content.ReadFromJsonAsync<ToDoTaskDto>();
        updatedTask.Should().NotBeNull();
        updatedTask.Title.Should().Be("Updated Title");
        updatedTask.IsDone.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateToDoTask_ShouldReturnBadRequest_WhenIdsDoNotMatch()
    {
        // Arrange
        var user = await CreateAuthenticatedUserAsync();
        var task = new ToDoTaskDto { Id = 1, Title = "Mismatch", UserId = user.Id };

        // Act - URL ID (2) != Body ID (1)
        var response = await _client.PutAsJsonAsync("/api/ToDoTasks/2", task);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Id in URL does not match Id in body");
    }

    [Fact]
    public async Task DeleteToDoTask_ShouldReturnNoContent_WhenTaskExists()
    {
        // Arrange
        var user = await CreateAuthenticatedUserAsync();

        // 1. Create task
        var createResponse = await _client.PostAsJsonAsync("/api/ToDoTasks",
            new ToDoTaskDto { Title = "To Delete", UserId = user.Id });

        // 2. CRITICAL: Check for success status code (201 Created)
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Expected 201 Created, but received {createResponse.StatusCode}. Content: {await createResponse.Content.ReadAsStringAsync()}");

        // 3. Read the content (stream is now valid for deserialization)
        var createdTask = await createResponse.Content.ReadFromJsonAsync<ToDoTaskDto>();

        // Assert that we got the object
        createdTask.Should().NotBeNull("The created task DTO was null. Check the JSON casing or controller logic.");

        // Act
        var response = await _client.DeleteAsync($"/api/ToDoTasks/{createdTask.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's gone
        var getResponse = await _client.GetAsync($"/api/ToDoTasks/{createdTask.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTasksListAsync_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null; // Ensure no token

        // Act
        var response = await _client.GetAsync("/api/ToDoTasks?userId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- Helper Method to Register, Login, and Set Header ---
    private async Task<UserDto> CreateAuthenticatedUserAsync()
    {
        // 1. Generate unique credentials
        var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
        var registerDto = new RegisterDto
        {
            DisplayName = $"User {uniqueId}",
            Email = $"user{uniqueId}@test.com",
            Password = "Password123!"
        };

        // 2. Register
        var registerResponse = await _client.PostAsJsonAsync("/api/Users", registerDto);

        registerResponse.EnsureSuccessStatusCode();

        // 3. Login
        var loginDto = new LoginDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/Users/login", loginDto);

        // DEBUG: Ensure login succeeded
        loginResponse.EnsureSuccessStatusCode();
        
        var userDto = await loginResponse.Content.ReadFromJsonAsync<UserDto>();

        // 4. Validate Token Existence
        userDto.Should().NotBeNull();
        userDto.Token.Should().NotBeNullOrEmpty("Authentication token was missing from the Login response");

        _client.DefaultRequestHeaders.Authorization = null;

        // 5. Set Header for this test context
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userDto.Token);

        return userDto;
    }
}