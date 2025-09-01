using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Api;
using ProjectManagement.Application.DTOs.Auth;
using ProjectManagement.Application.DTOs.Workspaces;
using System.Collections.Generic;

namespace ProjectManagement.Tests.Integration;

public class WorkspaceIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly CustomWebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;

  public WorkspaceIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
  public async Task Create_ValidData_ReturnsCreated()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateWorkspaceDto
    {
      Title = "My Test Workspace",
      Description = "A workspace for testing",
      Color = "#FF5733"
    };

    var response = await _client.PostAsJsonAsync("/api/workspace", createRequest);
    var createdWorkspace = await response.Content.ReadFromJsonAsync<WorkspaceDto>();

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(createdWorkspace);
    Assert.Equal("My Test Workspace", createdWorkspace.Title);
    Assert.Equal("A workspace for testing", createdWorkspace.Description);
    Assert.Equal("#FF5733", createdWorkspace.Color);
    Assert.NotEqual(Guid.Empty, createdWorkspace.Id);
  }

  [Fact]
  public async Task Create_MissingTitle_ReturnsBadRequest()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateWorkspaceDto
    {
      Title = "",
      Description = "Test Description"
    };

    var response = await _client.PostAsJsonAsync("/api/workspace", createRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task Create_InvalidColor_ReturnsBadRequest()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateWorkspaceDto
    {
      Title = "Test Workspace",
      Description = "Test Description",
      Color = "invalid-color"
    };

    var response = await _client.PostAsJsonAsync("/api/workspace", createRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetAll_WithExistingWorkspaces_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest1 = new CreateWorkspaceDto { Title = "Workspace 1", Description = "First workspace" };
    await _client.PostAsJsonAsync("/api/workspace", createRequest1);

    var createRequest2 = new CreateWorkspaceDto { Title = "Workspace 2", Description = "Second workspace" };
    await _client.PostAsJsonAsync("/api/workspace", createRequest2);

    var response = await _client.GetAsync("/api/workspace");
    var workspaces = await response.Content.ReadFromJsonAsync<List<WorkspaceDto>>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(workspaces);
    Assert.True(workspaces.Count >= 2);
    Assert.Contains(workspaces, w => w.Title == "Workspace 1");
    Assert.Contains(workspaces, w => w.Title == "Workspace 2");
  }

  [Fact]
  public async Task GetAll_WithNoWorkspaces_ReturnsOk()
  {
    var uniqueEmail = $"emptyuser{Guid.NewGuid()}@example.com";
    var token = await RegisterAndLoginTestUser(uniqueEmail);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var response = await _client.GetAsync("/api/workspace");
    var workspaces = await response.Content.ReadFromJsonAsync<List<WorkspaceDto>>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(workspaces);
    Assert.Empty(workspaces);
  }

  [Fact]
  public async Task Update_ValidData_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateWorkspaceDto
    {
      Title = "Original Title",
      Description = "Original Description",
      Color = "#FF0000"
    };
    var createResponse = await _client.PostAsJsonAsync("/api/workspace", createRequest);
    var createdWorkspace = await createResponse.Content.ReadFromJsonAsync<WorkspaceDto>();

    var updateRequest = new UpdateWorkspaceDto
    {
      Title = "Updated Title",
      Description = "Updated Description",
      Color = "#00FF00"
    };

    var response = await _client.PutAsJsonAsync($"/api/workspace/{createdWorkspace!.Id}", updateRequest);
    var updatedWorkspace = await response.Content.ReadFromJsonAsync<WorkspaceDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(updatedWorkspace);
    Assert.Equal(createdWorkspace.Id, updatedWorkspace.Id);
    Assert.Equal("Updated Title", updatedWorkspace.Title);
    Assert.Equal("Updated Description", updatedWorkspace.Description);
    Assert.Equal("#00FF00", updatedWorkspace.Color);
  }

  [Fact]
  public async Task Update_NonExistentWorkspace_ReturnsNotFound()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var updateRequest = new UpdateWorkspaceDto { Title = "Updated Title" };
    var nonExistentId = Guid.NewGuid();

    var response = await _client.PutAsJsonAsync($"/api/workspace/{nonExistentId}", updateRequest);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task Delete_ExistingWorkspace_ReturnsNoContent()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateWorkspaceDto { Title = "Workspace to Delete", Description = "This will be deleted" };
    var createResponse = await _client.PostAsJsonAsync("/api/workspace", createRequest);
    var createdWorkspace = await createResponse.Content.ReadFromJsonAsync<WorkspaceDto>();

    var response = await _client.DeleteAsync($"/api/workspace/{createdWorkspace!.Id}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var getResponse = await _client.GetAsync($"/api/workspace/{createdWorkspace.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task GetById_OtherUsersWorkspace_ReturnsForbidden()
  {
    var userAToken = await RegisterAndLoginTestUser("userA@example.com");
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAToken);

    var createRequest = new CreateWorkspaceDto
    {
      Title = "User A's Private Workspace",
      Description = "This workspace belongs to User A"
    };
    var createResponse = await _client.PostAsJsonAsync("/api/workspace", createRequest);
    var userAWorkspace = await createResponse.Content.ReadFromJsonAsync<WorkspaceDto>();

    var userBToken = await RegisterAndLoginTestUser("userB@example.com");
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userBToken);

    var response = await _client.GetAsync($"/api/workspace/{userAWorkspace!.Id}");

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }
}