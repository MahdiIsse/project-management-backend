using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Api;
using ProjectManagement.Application.DTOs;
using ProjectManagement.Application.DTOs.Assignees;
using ProjectManagement.Application.DTOs.Auth;

namespace ProjectManagement.Tests.Integration;

public class AssigneeIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly CustomWebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;

  public AssigneeIntegrationTests(CustomWebApplicationFactory<Program> factory)
  {
    _factory = factory;
    _client = _factory.CreateClient();
  }

  private async Task<string> RegisterAndLoginTestUser(string email = "test@example.com")
  {
    var registerRequest = new RegisterRequestDto { Email = email, Password = "TestPassword123!" };
    await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

    var loginRequest = new LoginRequestDto { Email = email, Password = "TestPassword123!" };
    var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
    Assert.True(loginResponse.IsSuccessStatusCode, $"Login failed: {await loginResponse.Content.ReadAsStringAsync()}");

    var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
    var token = loginResult!.Token;

    TestAuthHandler.RegisterUser(token, Guid.NewGuid().ToString(), email);
    return token;
  }

  [Fact]
  public async Task CreateAssignee_ValidData_ReturnsCreated()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateAssigneeDto
    {
      Name = "John Doe",
      AvatarUrl = "https://example.com/avatar.jpg"
    };

    var response = await _client.PostAsJsonAsync("/api/assignee", createRequest);
    var createdAssignee = await response.Content.ReadFromJsonAsync<AssigneeDto>();

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(createdAssignee);
    Assert.Equal("John Doe", createdAssignee.Name);
    Assert.Equal("https://example.com/avatar.jpg", createdAssignee.AvatarUrl);
    Assert.NotEqual(Guid.Empty, createdAssignee.Id);
  }

  [Fact]
  public async Task CreateAssignee_MissingName_ReturnsBadRequest()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateAssigneeDto
    {
      Name = "",
      AvatarUrl = "https://example.com/avatar.jpg"
    };

    var response = await _client.PostAsJsonAsync("/api/assignee", createRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetAllAssignees_WithExistingAssignees_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest1 = new CreateAssigneeDto { Name = "Assignee 1" };
    await _client.PostAsJsonAsync("/api/assignee", createRequest1);

    var createRequest2 = new CreateAssigneeDto { Name = "Assignee 2" };
    await _client.PostAsJsonAsync("/api/assignee", createRequest2);

    var response = await _client.GetAsync("/api/assignee");
    var assignees = await response.Content.ReadFromJsonAsync<List<AssigneeDto>>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(assignees);
    Assert.True(assignees.Count >= 2);
    Assert.Contains(assignees, a => a.Name == "Assignee 1");
    Assert.Contains(assignees, a => a.Name == "Assignee 2");
  }

  [Fact]
  public async Task GetAssigneeById_ExistingAssignee_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateAssigneeDto { Name = "Test Assignee" };
    var createResponse = await _client.PostAsJsonAsync("/api/assignee", createRequest);
    var createdAssignee = await createResponse.Content.ReadFromJsonAsync<AssigneeDto>();

    var response = await _client.GetAsync($"/api/assignee/{createdAssignee!.Id}");
    var assignee = await response.Content.ReadFromJsonAsync<AssigneeDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(assignee);
    Assert.Equal(createdAssignee.Id, assignee.Id);
    Assert.Equal("Test Assignee", assignee.Name);
  }

  [Fact]
  public async Task GetAssigneeById_NonExistentAssignee_ReturnsNotFound()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var nonExistentId = Guid.NewGuid();

    var response = await _client.GetAsync($"/api/assignee/{nonExistentId}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateAssignee_ValidData_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateAssigneeDto { Name = "Original Name" };
    var createResponse = await _client.PostAsJsonAsync("/api/assignee", createRequest);
    var createdAssignee = await createResponse.Content.ReadFromJsonAsync<AssigneeDto>();

    var updateRequest = new UpdateAssigneeDto
    {
      Name = "Updated Name",
      AvatarUrl = "https://example.com/new-avatar.jpg"
    };

    var response = await _client.PutAsJsonAsync($"/api/assignee/{createdAssignee!.Id}", updateRequest);
    var updatedAssignee = await response.Content.ReadFromJsonAsync<AssigneeDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(updatedAssignee);
    Assert.Equal(createdAssignee.Id, updatedAssignee.Id);
    Assert.Equal("Updated Name", updatedAssignee.Name);
    Assert.Equal("https://example.com/new-avatar.jpg", updatedAssignee.AvatarUrl);
  }

  [Fact]
  public async Task DeleteAssignee_ExistingAssignee_ReturnsNoContent()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateAssigneeDto { Name = "Assignee to Delete" };
    var createResponse = await _client.PostAsJsonAsync("/api/assignee", createRequest);
    var createdAssignee = await createResponse.Content.ReadFromJsonAsync<AssigneeDto>();

    var response = await _client.DeleteAsync($"/api/assignee/{createdAssignee!.Id}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var getResponse = await _client.GetAsync($"/api/assignee/{createdAssignee.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }
}