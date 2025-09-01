using System.Drawing;
using AutoMapper;
using ProjectManagement.Domain;
using ProjectManagement.Application.DTOs.Tags;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Infrastructure.Exceptions;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Application.Services;

public class TagService : ITagService
{
  private readonly ITagRepository _tagRepository;
  private readonly IMapper _mapper;

  public TagService(ITagRepository tagRepository, IMapper mapper)
  {
    _tagRepository = tagRepository;
    _mapper = mapper;
  }

  public async Task<IEnumerable<TagDto>> GetAllByUserIdAsync(string userId)
  {
    var tags = await _tagRepository.GetAllByUserIdAsync(userId);
    return _mapper.Map<IEnumerable<TagDto>>(tags);
  }

  public async Task<TagDto> GetByIdAsync(Guid id, string userId)
  {
    var tag = await _tagRepository.GetByIdAsync(id);
    if (tag == null) throw new NotFoundException("Tag", id);
    if (tag.UserId != userId) throw new ForbiddenException($"User does not have access to this tag: {id}");

    return _mapper.Map<TagDto>(tag);
  }

  public async Task<TagDto> CreateAsync(CreateTagDto createTagDto, string userId)
  {
    var tag = new Tag(
      name: createTagDto.Name,
      color: createTagDto.Color,
      userId: userId
    );

    var createdTag = await _tagRepository.CreateAsync(tag);
    return _mapper.Map<TagDto>(createdTag);
  }

  public async Task<TagDto> UpdateAsync(Guid id, UpdateTagDto updateTagDto, string userId)
  {
    var tag = await _tagRepository.GetByIdAsync(id);
    if (tag == null) throw new NotFoundException("Tag", id);
    if (tag.UserId != userId) throw new ForbiddenException($"User does not have access to this tag: {id}");

    tag.Update(
      name: updateTagDto.Name,
      color: updateTagDto.Color
    );

    var updatedTag = await _tagRepository.UpdateAsync(tag);
    return _mapper.Map<TagDto>(updatedTag);
  }

  public async Task DeleteAsync(Guid id, string userId)
  {
    var tag = await _tagRepository.GetByIdAsync(id);
    if (tag == null) throw new NotFoundException("Tag", id);
    if (tag.UserId != userId) throw new ForbiddenException($"User does not have access to this tag: {id}");

    await _tagRepository.DeleteAsync(tag);
  }
}