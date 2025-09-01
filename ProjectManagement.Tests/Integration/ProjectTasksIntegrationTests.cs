using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Api;
using ProjectManagement.Application.DTOs.Auth;
using ProjectManagement.Application.DTOs.Workspaces;
using ProjectManagement.Application.DTOs.Columns;
using ProjectManagement.Application.DTOs.Assignees;
using ProjectManagement.Application.DTOs.Tags;
using ProjectManagement.Application.DTOs.ProjectTasks;
using ProjectManagement.Domain;


namespace ProjectManagement.Tests.Integration;

public class ProjectTasksIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly CustomWebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;

  public ProjectTasksIntegrationTests(CustomWebApplicationFactory<Program> factory)
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

  private async Task<WorkspaceDto> CreateTestWorkspace(string token, string title = "Test Workspace")
  {
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createWorkspaceRequest = new CreateWorkspaceDto
    {
      Title = title,
      Description = "Workspace for task testing"
    };

    var response = await _client.PostAsJsonAsync("/api/workspace", createWorkspaceRequest);
    var workspace = await response.Content.ReadFromJsonAsync<WorkspaceDto>();

    return workspace!;
  }

  private async Task<ColumnDto> CreateTestColumn(string token, Guid workspaceId, string title = "Test Column")
  {
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createColumnRequest = new CreateColumnDto
    {
      Title = title,
      Color = "#FF5733"
    };

    var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspaceId}/columns", createColumnRequest);
    var column = await response.Content.ReadFromJsonAsync<ColumnDto>();

    return column!;
  }

  private async Task<AssigneeDto> CreateTestAssignee(string token, string name = "Test Assignee")
  {
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createAssigneeRequest = new CreateAssigneeDto
    {
      Name = name,
      AvatarUrl = null
    };

    var response = await _client.PostAsJsonAsync("/api/assignee", createAssigneeRequest);
    var assignee = await response.Content.ReadFromJsonAsync<AssigneeDto>();

    return assignee!;
  }

  private async Task<TagDto> CreateTestTag(string token, string name = "Test Tag")
  {
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createTagRequest = new CreateTagDto
    {
      Name = name,
      Color = "#FF5733"
    };

    var response = await _client.PostAsJsonAsync("/api/tag", createTagRequest);
    var tag = await response.Content.ReadFromJsonAsync<TagDto>();

    return tag!;
  }

  [Fact]
  public async Task CreateTask_ValidData_ReturnsCreated()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    var column = await CreateTestColumn(token, workspace.Id);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateProjectTaskDto
    {
      Title = "My Test Task",
      Description = "A task for testing",
      Priority = TaskPriority.Medium,
      DueDate = DateTime.UtcNow.AddDays(7)
    };

    var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={column.Id}", createRequest);
    var createdTask = await response.Content.ReadFromJsonAsync<ProjectTaskDto>();

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(createdTask);
    Assert.Equal("My Test Task", createdTask.Title);
    Assert.Equal("A task for testing", createdTask.Description);
    Assert.Equal(TaskPriority.Medium, createdTask.Priority);
    Assert.Equal(workspace.Id, createdTask.WorkspaceId);
    Assert.Equal(column.Id, createdTask.ColumnId);
    Assert.NotEqual(Guid.Empty, createdTask.Id);
  }

  [Fact]
  public async Task GetTasks_WithExistingTasks_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    var column = await CreateTestColumn(token, workspace.Id);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest1 = new CreateProjectTaskDto { Title = "Task 1", Priority = TaskPriority.Low };
    await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={column.Id}", createRequest1);

    var createRequest2 = new CreateProjectTaskDto { Title = "Task 2", Priority = TaskPriority.High };
    await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={column.Id}", createRequest2);

    var response = await _client.GetAsync($"/api/workspaces/{workspace.Id}/tasks");
    var tasks = await response.Content.ReadFromJsonAsync<List<ProjectTaskDto>>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(tasks);
    Assert.True(tasks.Count >= 2);
    Assert.Contains(tasks, t => t.Title == "Task 1");
    Assert.Contains(tasks, t => t.Title == "Task 2");
  }

  [Fact]
  public async Task GetTaskById_ExistingTask_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    var column = await CreateTestColumn(token, workspace.Id);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateProjectTaskDto
    {
      Title = "Test Task for GetById",
      Priority = TaskPriority.Medium,
      Description = "Testing GetById functionality"
    };
    var createResponse = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={column.Id}", createRequest);
    var createdTask = await createResponse.Content.ReadFromJsonAsync<ProjectTaskDto>();

    var response = await _client.GetAsync($"/api/workspaces/{workspace.Id}/tasks/{createdTask!.Id}");
    var task = await response.Content.ReadFromJsonAsync<ProjectTaskDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(task);
    Assert.Equal(createdTask.Id, task.Id);
    Assert.Equal("Test Task for GetById", task.Title);
    Assert.Equal(TaskPriority.Medium, task.Priority);
  }

  [Fact]
  public async Task UpdateTask_ValidData_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    var column = await CreateTestColumn(token, workspace.Id);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateProjectTaskDto
    {
      Title = "Original Task",
      Priority = TaskPriority.Low,
      Description = "Original description"
    };
    var createResponse = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={column.Id}", createRequest);
    var createdTask = await createResponse.Content.ReadFromJsonAsync<ProjectTaskDto>();

    var updateRequest = new UpdateProjectTaskDto
    {
      Title = "Updated Task",
      Priority = TaskPriority.High,
      Description = "Updated description",
      Position = 2,
      DueDate = DateTime.UtcNow.AddDays(3)
    };

    var response = await _client.PutAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks/{createdTask!.Id}", updateRequest);
    var updatedTask = await response.Content.ReadFromJsonAsync<ProjectTaskDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(updatedTask);
    Assert.Equal(createdTask.Id, updatedTask.Id);
    Assert.Equal("Updated Task", updatedTask.Title);
    Assert.Equal(TaskPriority.High, updatedTask.Priority);
    Assert.Equal("Updated description", updatedTask.Description);
    Assert.Equal(2, updatedTask.Position);
  }

  [Fact]
  public async Task DeleteTask_ExistingTask_ReturnsNoContent()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    var column = await CreateTestColumn(token, workspace.Id);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateProjectTaskDto { Title = "Task to Delete", Priority = TaskPriority.Medium };
    var createResponse = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={column.Id}", createRequest);
    var createdTask = await createResponse.Content.ReadFromJsonAsync<ProjectTaskDto>();

    var response = await _client.DeleteAsync($"/api/workspaces/{workspace.Id}/tasks/{createdTask!.Id}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var getResponse = await _client.GetAsync($"/api/workspaces/{workspace.Id}/tasks/{createdTask.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task CreateTask_MissingTitle_ReturnsBadRequest()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    var column = await CreateTestColumn(token, workspace.Id);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateProjectTaskDto
    {
      Title = "",
      Priority = TaskPriority.Medium
    };

    var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={column.Id}", createRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task CreateTask_InvalidPriority_ReturnsBadRequest()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    var column = await CreateTestColumn(token, workspace.Id);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateProjectTaskDto
    {
      Title = "Test Task",
      Priority = (TaskPriority)999
    };

    var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={column.Id}", createRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task CreateTask_InvalidColumnId_ReturnsNotFound()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateProjectTaskDto
    {
      Title = "Test Task",
      Priority = TaskPriority.Medium
    };

    var nonExistentColumnId = Guid.NewGuid();

    var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={nonExistentColumnId}", createRequest);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateTask_NonExistentTask_ReturnsNotFound()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var updateRequest = new UpdateProjectTaskDto
    {
      Title = "Updated Title",
      Priority = TaskPriority.High,
      Position = 1
    };

    var nonExistentTaskId = Guid.NewGuid();

    var response = await _client.PutAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks/{nonExistentTaskId}", updateRequest);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task AddAssigneeToTask_ValidData_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    var column = await CreateTestColumn(token, workspace.Id);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createTaskRequest = new CreateProjectTaskDto { Title = "Task with Assignee", Priority = TaskPriority.Medium };
    var taskResponse = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={column.Id}", createTaskRequest);
    var task = await taskResponse.Content.ReadFromJsonAsync<ProjectTaskDto>();

    var assignee = await CreateTestAssignee(token, "Test Assignee");

    var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks/{task!.Id}/assignees/{assignee.Id}", new { });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task AddTagToTask_ValidData_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    var column = await CreateTestColumn(token, workspace.Id);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createTaskRequest = new CreateProjectTaskDto { Title = "Task with Tag", Priority = TaskPriority.Medium };
    var taskResponse = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks?columnId={column.Id}", createTaskRequest);
    var task = await taskResponse.Content.ReadFromJsonAsync<ProjectTaskDto>();

    var tag = await CreateTestTag(token, "Test Tag");

    var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/tasks/{task!.Id}/tags/{tag.Id}", new { });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }
}