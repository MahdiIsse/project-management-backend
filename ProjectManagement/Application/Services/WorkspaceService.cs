using AutoMapper;
using ProjectManagement.Domain;
using ProjectManagement.Application.DTOs.Workspaces;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Infrastructure.Exceptions;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Application.Services;

public class WorkspaceService : IWorkspaceService
{
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly IMapper _mapper;
  private readonly ILogger<WorkspaceService> _logger;

  public WorkspaceService(
    IWorkspaceRepository workspaceRepository,
    IMapper mapper,
    ILogger<WorkspaceService> logger)
  {
    _workspaceRepository = workspaceRepository;
    _mapper = mapper;
    _logger = logger;
  }

  public async Task<IEnumerable<WorkspaceDto>> GetAllAsync(string userId)
  {
    _logger.LogInformation("Service: Getting all workspaces for user {UserId}", userId);

    var workspaces = await _workspaceRepository.GetAllByUserIdAsync(userId);
    var workspaceList = workspaces.ToList();

    _logger.LogInformation("Service: Retrieved {Count} workspaces for user {UserId}",
      workspaceList.Count, userId);

    return _mapper.Map<IEnumerable<WorkspaceDto>>(workspaceList);
  }

  public async Task<WorkspaceDto> GetByIdAsync(Guid id, string userId)
  {
    _logger.LogInformation("Service: Getting workspace {WorkspaceId} for user {UserId}", id, userId);

    var workspace = await _workspaceRepository.GetByIdAsync(id);
    if (workspace == null)
    {
      _logger.LogWarning("Service: Workspace {WorkspaceId} not found", id);
      throw new NotFoundException("Workspace", id);
    }

    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(id, userId))
    {
      _logger.LogWarning("Service: User {UserId} does not have access to workspace {WorkspaceId}", userId, id);
      throw new ForbiddenException($"User does not have access to workspace: {id}");
    }

    _logger.LogInformation("Service: Retrieved workspace {WorkspaceId} with title '{Title}' for user {UserId}",
      id, workspace.Title, userId);

    return _mapper.Map<WorkspaceDto>(workspace);
  }

  public async Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto createWorkspaceDto, string userId)
  {
    _logger.LogInformation("Service: Creating workspace '{Title}' for user {UserId}",
      createWorkspaceDto.Title, userId);

    var maxPosition = await _workspaceRepository.GetMaxPositionAsync(userId);

    var workspace = new Workspace(
      title: createWorkspaceDto.Title,
      description: createWorkspaceDto.Description,
      color: createWorkspaceDto.Color,
      userId: userId
    );

    workspace.SetPosition(maxPosition + 1);

    var createdWorkspace = await _workspaceRepository.CreateAsync(workspace);

    _logger.LogInformation("Service: Created workspace {WorkspaceId} with title '{Title}' for user {UserId}",
      createdWorkspace.Id, createdWorkspace.Title, userId);

    return _mapper.Map<WorkspaceDto>(createdWorkspace);
  }

  public async Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto updateWorkspaceDto, string userId)
  {
    _logger.LogInformation("Service: Updating workspace {WorkspaceId} for user {UserId}", id, userId);

    var workspace = await _workspaceRepository.GetByIdAsync(id);
    if (workspace == null)
    {
      _logger.LogWarning("Service: Cannot update workspace {WorkspaceId} - not found", id);
      throw new NotFoundException("Workspace", id);
    }

    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(id, userId))
    {
      _logger.LogWarning("Service: User {UserId} does not have access to update workspace {WorkspaceId}", userId, id);
      throw new ForbiddenException($"User does not have access to workspace: {id}");
    }

    workspace.Update(
      title: updateWorkspaceDto.Title,
      description: updateWorkspaceDto.Description,
      color: updateWorkspaceDto.Color,
      position: updateWorkspaceDto.Position
    );

    var updatedWorkspace = await _workspaceRepository.UpdateAsync(workspace);

    _logger.LogInformation("Service: Updated workspace {WorkspaceId} successfully for user {UserId}", id, userId);

    return _mapper.Map<WorkspaceDto>(updatedWorkspace);
  }

  public async Task DeleteAsync(Guid id, string userId)
  {
    _logger.LogInformation("Service: Deleting workspace {WorkspaceId} for user {UserId}", id, userId);

    var workspace = await _workspaceRepository.GetByIdAsync(id);
    if (workspace == null)
    {
      _logger.LogWarning("Service: Cannot delete workspace {WorkspaceId} - not found", id);
      throw new NotFoundException("Workspace", id);
    }

    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(id, userId))
    {
      _logger.LogWarning("Service: User {UserId} does not have access to delete workspace {WorkspaceId}", userId, id);
      throw new ForbiddenException($"User does not have access to workspace: {id}");
    }

    await _workspaceRepository.DeleteAsync(workspace);

    _logger.LogInformation("Service: Deleted workspace {WorkspaceId} successfully for user {UserId}", id, userId);
  }
}