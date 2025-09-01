using ProjectManagement.Application.DTOs.Assignees;

namespace ProjectManagement.Application.Interfaces.Services;

public interface IAssigneeService
{
  Task<IEnumerable<AssigneeDto>> GetAllByUserIdAsync(string userId);
  Task<AssigneeDto> GetByIdAsync(Guid id, string userId);
  Task<AssigneeDto> CreateAsync(CreateAssigneeDto createAssigneeDto, string userId);
  Task<AssigneeDto> UpdateAsync(Guid id, UpdateAssigneeDto updateAssigneeDto, string userId);
  Task DeleteAsync(Guid id, string userId);
}