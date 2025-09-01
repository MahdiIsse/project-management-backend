using AutoMapper;
using FluentAssertions;
using Moq;
using ProjectManagement.Domain;
using ProjectManagement.Application.DTOs.Tags;
using ProjectManagement.Application.Services;
using ProjectManagement.Infrastructure.Exceptions;
using ProjectManagement.Infrastructure.Mappings;
using ProjectManagement.Application.Interfaces.Repositories;

namespace ProjectManagement.Tests.Services;

public class TagServiceTests
{
  private readonly Mock<ITagRepository> _tagRepository;
  private readonly IMapper _mapper;
  private readonly TagService _tagService;

  public TagServiceTests()
  {
    _tagRepository = new Mock<ITagRepository>();

    var config = new MapperConfiguration(cfg =>
    {
      cfg.AddProfile<AutoMapperProfiles>();
    });
    _mapper = config.CreateMapper();

    _tagService = new TagService(
        _tagRepository.Object,
        _mapper
    );
  }

  [Fact]
  public async Task GetAllByUserIdAsync_WithValidUserId_ShouldReturnMappedTags()
  {
    var userId = "user123";
    var tags = new List<Tag>
        {
            CreateTestTag("Frontend", "#FF0000", userId),
            CreateTestTag("Backend", "#00FF00", userId)
        };

    _tagRepository.Setup(r => r.GetAllByUserIdAsync(userId))
        .ReturnsAsync(tags);

    var result = await _tagService.GetAllByUserIdAsync(userId);

    result.Should().NotBeNull();
    result.Should().HaveCount(2);

    var resultList = result.ToList();
    resultList[0].Name.Should().Be("Frontend");
    resultList[0].Color.Should().Be("#FF0000");
    resultList[1].Name.Should().Be("Backend");
    resultList[1].Color.Should().Be("#00FF00");

    _tagRepository.Verify(r => r.GetAllByUserIdAsync(userId), Times.Once);
  }

  [Fact]
  public async Task GetAllByUserIdAsync_WithEmptyResult_ShouldReturnEmptyCollection()
  {
    var userId = "user123";
    var emptyTags = new List<Tag>();

    _tagRepository.Setup(r => r.GetAllByUserIdAsync(userId))
        .ReturnsAsync(emptyTags);

    var result = await _tagService.GetAllByUserIdAsync(userId);

    result.Should().NotBeNull();
    result.Should().BeEmpty();

    _tagRepository.Verify(r => r.GetAllByUserIdAsync(userId), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithValidIdAndAccess_ShouldReturnTagDto()
  {
    var tagId = Guid.NewGuid();
    var userId = "user123";
    var tag = CreateTestTag("Test Tag", "#FF0000", userId);
    typeof(Tag).GetProperty("Id")!.SetValue(tag, tagId);

    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync(tag);

    var result = await _tagService.GetByIdAsync(tagId, userId);

    result.Should().NotBeNull();
    result.Id.Should().Be(tagId);
    result.Name.Should().Be("Test Tag");
    result.Color.Should().Be("#FF0000");

    _tagRepository.Verify(r => r.GetByIdAsync(tagId), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var tagId = Guid.NewGuid();
    var userId = "user123";

    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync((Tag?)null);

    await _tagService.Invoking(s => s.GetByIdAsync(tagId, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Tag with ID '{tagId}' was not found.");

    _tagRepository.Verify(r => r.GetByIdAsync(tagId), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var tagId = Guid.NewGuid();
    var userId = "user123";
    var differentUserId = "differentUser";
    var tag = CreateTestTag("Test Tag", "#FF0000", differentUserId);

    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync(tag);

    await _tagService.Invoking(s => s.GetByIdAsync(tagId, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to this tag: {tagId}");

    _tagRepository.Verify(r => r.GetByIdAsync(tagId), Times.Once);
  }

  [Fact]
  public async Task CreateAsync_WithValidData_ShouldCreateAndReturnTagDto()
  {
    var userId = "user123";
    var createDto = CreateTestCreateDto("New Tag", "#123456");

    var createdTag = CreateTestTag("New Tag", "#123456", userId);
    _tagRepository.Setup(r => r.CreateAsync(It.IsAny<Tag>()))
        .ReturnsAsync(createdTag);

    var result = await _tagService.CreateAsync(createDto, userId);

    result.Should().NotBeNull();
    result.Name.Should().Be("New Tag");
    result.Color.Should().Be("#123456");

    _tagRepository.Verify(r => r.CreateAsync(It.Is<Tag>(t =>
        t.Name == "New Tag" &&
        t.Color == "#123456" &&
        t.UserId == userId
    )), Times.Once);
  }

  [Fact]
  public async Task UpdateAsync_WithValidDataAndAccess_ShouldUpdateAndReturnTagDto()
  {
    var tagId = Guid.NewGuid();
    var userId = "user123";
    var existingTag = CreateTestTag("Old Name", "#FF0000", userId);
    typeof(Tag).GetProperty("Id")!.SetValue(existingTag, tagId);

    var updateDto = CreateTestUpdateDto("Updated Name", "#00FF00");

    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync(existingTag);
    _tagRepository.Setup(r => r.UpdateAsync(It.IsAny<Tag>()))
        .ReturnsAsync(existingTag);

    var result = await _tagService.UpdateAsync(tagId, updateDto, userId);

    result.Should().NotBeNull();
    result.Name.Should().Be("Updated Name");
    result.Color.Should().Be("#00FF00");

    _tagRepository.Verify(r => r.GetByIdAsync(tagId), Times.Once);
    _tagRepository.Verify(r => r.UpdateAsync(It.Is<Tag>(t =>
        t.Name == "Updated Name" &&
        t.Color == "#00FF00"
    )), Times.Once);
  }

  [Fact]
  public async Task UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var tagId = Guid.NewGuid();
    var userId = "user123";
    var updateDto = CreateTestUpdateDto("Updated Name", "#00FF00");

    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync((Tag?)null);

    await _tagService.Invoking(s => s.UpdateAsync(tagId, updateDto, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Tag with ID '{tagId}' was not found.");

    _tagRepository.Verify(r => r.GetByIdAsync(tagId), Times.Once);
    _tagRepository.Verify(r => r.UpdateAsync(It.IsAny<Tag>()), Times.Never);
  }

  [Fact]
  public async Task UpdateAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var tagId = Guid.NewGuid();
    var userId = "user123";
    var differentUserId = "differentUser";
    var existingTag = CreateTestTag("Test Tag", "#FF0000", differentUserId);
    var updateDto = CreateTestUpdateDto("New Name", "#00FF00");

    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync(existingTag);

    await _tagService.Invoking(s => s.UpdateAsync(tagId, updateDto, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to this tag: {tagId}");

    _tagRepository.Verify(r => r.GetByIdAsync(tagId), Times.Once);
    _tagRepository.Verify(r => r.UpdateAsync(It.IsAny<Tag>()), Times.Never);
  }

  [Fact]
  public async Task DeleteAsync_WithValidIdAndAccess_ShouldDeleteTag()
  {
    var tagId = Guid.NewGuid();
    var userId = "user123";
    var tag = CreateTestTag("Test Tag", "#FF0000", userId);

    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync(tag);

    await _tagService.DeleteAsync(tagId, userId);

    _tagRepository.Verify(r => r.GetByIdAsync(tagId), Times.Once);
    _tagRepository.Verify(r => r.DeleteAsync(tag), Times.Once);
  }

  [Fact]
  public async Task DeleteAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var tagId = Guid.NewGuid();
    var userId = "user123";

    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync((Tag?)null);

    await _tagService.Invoking(s => s.DeleteAsync(tagId, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Tag with ID '{tagId}' was not found.");

    _tagRepository.Verify(r => r.GetByIdAsync(tagId), Times.Once);
    _tagRepository.Verify(r => r.DeleteAsync(It.IsAny<Tag>()), Times.Never);
  }

  [Fact]
  public async Task DeleteAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var tagId = Guid.NewGuid();
    var userId = "user123";
    var differentUserId = "differentUser";
    var tag = CreateTestTag("Test Tag", "#FF0000", differentUserId);

    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync(tag);

    await _tagService.Invoking(s => s.DeleteAsync(tagId, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to this tag: {tagId}");

    _tagRepository.Verify(r => r.GetByIdAsync(tagId), Times.Once);
    _tagRepository.Verify(r => r.DeleteAsync(It.IsAny<Tag>()), Times.Never);
  }

  private static Tag CreateTestTag(string name, string color, string userId)
  {
    return new Tag(name, color, userId);
  }

  private static CreateTagDto CreateTestCreateDto(string name, string color)
  {
    return new CreateTagDto
    {
      Name = name,
      Color = color
    };
  }

  private static UpdateTagDto CreateTestUpdateDto(string name, string color)
  {
    return new UpdateTagDto
    {
      Name = name,
      Color = color
    };
  }
}
