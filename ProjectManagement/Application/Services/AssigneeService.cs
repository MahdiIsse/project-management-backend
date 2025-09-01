using AutoMapper;
using ProjectManagement.Domain;
using ProjectManagement.Application.DTOs.Assignees;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Infrastructure.Exceptions;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Application.Services;

public class AssigneeService : IAssigneeService
{
  private readonly IAssigneeRepository _assigneeRepository;
  private readonly IMapper _mapper;
  public AssigneeService(IAssigneeRepository assigneeRepository, IMapper mapper)
  {
    _assigneeRepository = assigneeRepository;
    _mapper = mapper;
  }
  public async Task<IEnumerable<AssigneeDto>> GetAllByUserIdAsync(string userId)
  {
    var assignees = await _assigneeRepository.GetAllByUserIdAsync(userId);

    return _mapper.Map<IEnumerable<AssigneeDto>>(assignees);
  }

  public async Task<AssigneeDto> GetByIdAsync(Guid id, string userId)
  {
    var assignee = await _assigneeRepository.GetByIdAsync(id);
    if (assignee == null) throw new NotFoundException("Assignee", id);
    if (assignee.UserId != userId) throw new ForbiddenException($"User does not have access to assignee: {id}");

    return _mapper.Map<AssigneeDto>(assignee);
  }

  public async Task<AssigneeDto> CreateAsync(CreateAssigneeDto createAssigneeDto, string userId)
  {
    var assignee = new Assignee(
      name: createAssigneeDto.Name,
      avatarUrl: createAssigneeDto.AvatarUrl,
      userId: userId
    );

    var createdAssignee = await _assigneeRepository.CreateAsync(assignee);
    return _mapper.Map<AssigneeDto>(createdAssignee);
  }

  public async Task<AssigneeDto> UpdateAsync(Guid id, UpdateAssigneeDto updateAssigneeDto, string userId)
  {
    var assignee = await _assigneeRepository.GetByIdAsync(id);
    if (assignee == null) throw new NotFoundException("Assignee", id);
    if (assignee.UserId != userId) throw new ForbiddenException($"User does not have access to assignee: {id}");

    assignee.Update(
      name: updateAssigneeDto.Name,
      avatarUrl: updateAssigneeDto.AvatarUrl
    );

    var updatedAssignee = await _assigneeRepository.UpdateAsync(assignee);
    return _mapper.Map<AssigneeDto>(updatedAssignee);
  }

  public async Task DeleteAsync(Guid id, string userId)
  {
    var assignee = await _assigneeRepository.GetByIdAsync(id);
    if (assignee == null) throw new NotFoundException("Assignee", id);
    if (assignee.UserId != userId) throw new ForbiddenException($"User does not have access to assignee: {id}");

    await _assigneeRepository.DeleteAsync(assignee);
  }
}