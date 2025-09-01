using ProjectManagement.Application.DTOs.Columns;

namespace ProjectManagement.Application.Interfaces.Services;

public interface IColumnService
{
  Task<IEnumerable<ColumnDto>> GetAllAsync(Guid workspaceId, string userId);
  Task<ColumnDto> GetByIdAsync(Guid id, string userId);
  Task<ColumnDto> CreateAsync(Guid workspaceId, CreateColumnDto createColumnDto, string userId);
  Task<ColumnDto> UpdateAsync(Guid id, UpdateColumnDto updateColumnDto, string userId);
  Task DeleteAsync(Guid id, string userId);
}