using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Api;
using ProjectManagement.Application.DTOs.Auth;
using ProjectManagement.Application.DTOs.Tags;

namespace ProjectManagement.Tests.Integration;

public class TagIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly CustomWebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;

  public TagIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
  public async Task CreateTag_ValidData_ReturnsCreated()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateTagDto
    {
      Name = "Urgent",
      Color = "#FF5733"
    };

    var response = await _client.PostAsJsonAsync("/api/tag", createRequest);
    var createdTag = await response.Content.ReadFromJsonAsync<TagDto>();

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(createdTag);
    Assert.Equal("Urgent", createdTag.Name);
    Assert.Equal("#FF5733", createdTag.Color);
    Assert.NotEqual(Guid.Empty, createdTag.Id);
  }

  [Fact]
  public async Task CreateTag_MissingName_ReturnsBadRequest()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateTagDto
    {
      Name = "",
      Color = "#FF5733"
    };

    var response = await _client.PostAsJsonAsync("/api/tag", createRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task CreateTag_InvalidColor_ReturnsBadRequest()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateTagDto
    {
      Name = "Test Tag",
      Color = "invalid-color"
    };

    var response = await _client.PostAsJsonAsync("/api/tag", createRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetAllTags_WithExistingTags_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest1 = new CreateTagDto { Name = "Tag 1", Color = "#FF0000" };
    await _client.PostAsJsonAsync("/api/tag", createRequest1);

    var createRequest2 = new CreateTagDto { Name = "Tag 2", Color = "#00FF00" };
    await _client.PostAsJsonAsync("/api/tag", createRequest2);

    var response = await _client.GetAsync("/api/tag");
    var tags = await response.Content.ReadFromJsonAsync<List<TagDto>>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(tags);
    Assert.True(tags.Count >= 2);
    Assert.Contains(tags, t => t.Name == "Tag 1");
    Assert.Contains(tags, t => t.Name == "Tag 2");
  }

  [Fact]
  public async Task GetTagById_ExistingTag_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateTagDto { Name = "Test Tag", Color = "#FF5733" };
    var createResponse = await _client.PostAsJsonAsync("/api/tag", createRequest);
    var createdTag = await createResponse.Content.ReadFromJsonAsync<TagDto>();

    var response = await _client.GetAsync($"/api/tag/{createdTag!.Id}");
    var tag = await response.Content.ReadFromJsonAsync<TagDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(tag);
    Assert.Equal(createdTag.Id, tag.Id);
    Assert.Equal("Test Tag", tag.Name);
    Assert.Equal("#FF5733", tag.Color);
  }

  [Fact]
  public async Task UpdateTag_ValidData_ReturnsOk()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateTagDto { Name = "Original Tag", Color = "#FF0000" };
    var createResponse = await _client.PostAsJsonAsync("/api/tag", createRequest);
    var createdTag = await createResponse.Content.ReadFromJsonAsync<TagDto>();

    var updateRequest = new UpdateTagDto
    {
      Name = "Updated Tag",
      Color = "#00FF00"
    };

    var response = await _client.PutAsJsonAsync($"/api/tag/{createdTag!.Id}", updateRequest);
    var updatedTag = await response.Content.ReadFromJsonAsync<TagDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(updatedTag);
    Assert.Equal(createdTag.Id, updatedTag.Id);
    Assert.Equal("Updated Tag", updatedTag.Name);
    Assert.Equal("#00FF00", updatedTag.Color);
  }

  [Fact]
  public async Task DeleteTag_ExistingTag_ReturnsNoContent()
  {
    var token = await RegisterAndLoginTestUser();
    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var createRequest = new CreateTagDto { Name = "Tag to Delete", Color = "#FF5733" };
    var createResponse = await _client.PostAsJsonAsync("/api/tag", createRequest);
    var createdTag = await createResponse.Content.ReadFromJsonAsync<TagDto>();

    var response = await _client.DeleteAsync($"/api/tag/{createdTag!.Id}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var getResponse = await _client.GetAsync($"/api/tag/{createdTag.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }
}