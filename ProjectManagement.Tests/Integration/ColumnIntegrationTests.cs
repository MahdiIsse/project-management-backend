using System.Net;
using System.Net.Http.Json;
using ProjectManagement.Application.DTOs.Auth;
using ProjectManagement.Application.DTOs.Workspaces;
using ProjectManagement.Application.DTOs.Columns;


namespace ProjectManagement.Tests.Integration;

public class ColumnIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly CustomWebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;

  public ColumnIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
      Description = "Workspace for column testing"
    };

    var response = await _client.PostAsJsonAsync("/api/workspace", createWorkspaceRequest);
    var workspace = await response.Content.ReadFromJsonAsync<WorkspaceDto>();

    return workspace!;
  }

  [Fact]
  public async Task CreateColumn_ValidData_ReturnsCreated()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateColumnDto
    {
      Title = "My Test Column",
      Color = "#FF5733"
    };

    var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/columns", createRequest);
    var createdColumn = await response.Content.ReadFromJsonAsync<ColumnDto>();

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(createdColumn);
    Assert.Equal("My Test Column", createdColumn.Title);
    Assert.Equal("#FF5733", createdColumn.Color);
    Assert.Equal(workspace.Id, createdColumn.WorkspaceId);
    Assert.NotEqual(Guid.Empty, createdColumn.Id);
  }

  [Fact]
  public async Task CreateColumn_MissingTitle_ReturnsBadRequest()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateColumnDto
    {
      Title = "",
      Color = "#FF5733"
    };

    var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/columns", createRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task CreateColumn_InvalidColor_ReturnsBadRequest()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateColumnDto
    {
      Title = "Test Column",
      Color = "invalid-color"
    };

    var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/columns", createRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetAllColumns_WithExistingColumns_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest1 = new CreateColumnDto { Title = "Column 1", Color = "#FF0000" };
    await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/columns", createRequest1);

    var createRequest2 = new CreateColumnDto { Title = "Column 2", Color = "#00FF00" };
    await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/columns", createRequest2);

    var response = await _client.GetAsync($"/api/workspaces/{workspace.Id}/columns");
    var columns = await response.Content.ReadFromJsonAsync<List<ColumnDto>>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(columns);
    Assert.True(columns.Count >= 2);
    Assert.Contains(columns, c => c.Title == "Column 1");
    Assert.Contains(columns, c => c.Title == "Column 2");
  }

  [Fact]
  public async Task GetAllColumns_WithNoColumns_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token, "Empty Workspace");
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var response = await _client.GetAsync($"/api/workspaces/{workspace.Id}/columns");
    var columns = await response.Content.ReadFromJsonAsync<List<ColumnDto>>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(columns);
    Assert.Empty(columns);
  }

  [Fact]
  public async Task UpdateColumn_ValidData_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateColumnDto
    {
      Title = "Original Column",
      Color = "#FF0000"
    };
    var createResponse = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/columns", createRequest);
    var createdColumn = await createResponse.Content.ReadFromJsonAsync<ColumnDto>();

    var updateRequest = new UpdateColumnDto
    {
      Title = "Updated Column",
      Color = "#00FF00",
      Position = 1
    };

    var response = await _client.PutAsJsonAsync($"/api/workspaces/{workspace.Id}/columns/{createdColumn!.Id}", updateRequest);
    var updatedColumn = await response.Content.ReadFromJsonAsync<ColumnDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(updatedColumn);
    Assert.Equal(createdColumn.Id, updatedColumn.Id);
    Assert.Equal("Updated Column", updatedColumn.Title);
    Assert.Equal("#00FF00", updatedColumn.Color);
    Assert.Equal(1, updatedColumn.Position);
  }

  [Fact]
  public async Task UpdateColumn_NonExistentColumn_ReturnsNotFound()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var updateRequest = new UpdateColumnDto { Title = "Updated Title" };
    var nonExistentId = Guid.NewGuid();

    var response = await _client.PutAsJsonAsync($"/api/workspaces/{workspace.Id}/columns/{nonExistentId}", updateRequest);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task DeleteColumn_ExistingColumn_ReturnsNoContent()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateColumnDto { Title = "Column to Delete", Color = "#FF5733" };
    var createResponse = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/columns", createRequest);
    var createdColumn = await createResponse.Content.ReadFromJsonAsync<ColumnDto>();

    var response = await _client.DeleteAsync($"/api/workspaces/{workspace.Id}/columns/{createdColumn!.Id}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var getResponse = await _client.GetAsync($"/api/workspaces/{workspace.Id}/columns/{createdColumn.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task CreateColumn_PositionAutoIncrement_ReturnsCorrectPosition()
  {
    var token = await RegisterAndLoginTestUser();
    var workspace = await CreateTestWorkspace(token);
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest1 = new CreateColumnDto { Title = "First Column" };
    var response1 = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/columns", createRequest1);
    var column1 = await response1.Content.ReadFromJsonAsync<ColumnDto>();

    var createRequest2 = new CreateColumnDto { Title = "Second Column" };
    var response2 = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/columns", createRequest2);
    var column2 = await response2.Content.ReadFromJsonAsync<ColumnDto>();

    Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
    Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
    Assert.NotNull(column1);
    Assert.NotNull(column2);
    Assert.Equal(0, column1.Position);
    Assert.Equal(1, column2.Position);
  }
}