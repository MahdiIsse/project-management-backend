using AutoMapper;
using ProjectManagement.Application.DTOs.ProjectTasks;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Infrastructure.Exceptions;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Domain;



namespace ProjectManagement.Application.Services;

public class ProjectTaskService : IProjectTaskService
{
  private readonly IProjectTaskRepository _projectTaskRepository;
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly IColumnRepository _columnRepository;
  private readonly IAssigneeRepository _assigneeRepository;
  private readonly ITagRepository _tagRepository;
  private readonly IMapper _mapper;
  private readonly ILogger<ProjectTaskService> _logger;

  public ProjectTaskService(
    IProjectTaskRepository projectTaskRepository,
    IWorkspaceRepository workspaceRepository,
    IColumnRepository columnRepository,
    IAssigneeRepository assigneeRepository,
    ITagRepository tagRepository,
    IMapper mapper,
    ILogger<ProjectTaskService> logger)
  {
    _projectTaskRepository = projectTaskRepository;
    _workspaceRepository = workspaceRepository;
    _columnRepository = columnRepository;
    _assigneeRepository = assigneeRepository;
    _tagRepository = tagRepository;
    _mapper = mapper;
    _logger = logger;
  }

  public async Task<IEnumerable<ProjectTaskDto>> GetAllByWorkspaceIdAsync(Guid workspaceId, string userId)
  {
    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(workspaceId, userId))
    {
      throw new ForbiddenException($"User does not have access to workspace: {workspaceId}");
    }

    var tasks = await _projectTaskRepository.GetAllByWorkspaceIdAsync(workspaceId);
    return _mapper.Map<IEnumerable<ProjectTaskDto>>(tasks);
  }

  public async Task<ProjectTaskDto> GetByIdAsync(Guid taskId, string userId)
  {
    var task = await _projectTaskRepository.GetByIdAsync(taskId);
    if (task == null) throw new NotFoundException("Task", taskId);

    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(task.WorkspaceId, userId))
    {
      throw new ForbiddenException($"User does not have access to task: {taskId}");
    }
    return _mapper.Map<ProjectTaskDto>(task);
  }

  public async Task<ProjectTaskDto> CreateAsync(Guid workspaceId, Guid columnId, CreateProjectTaskDto createProjectTaskDto, string userId)
  {
    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(workspaceId, userId))
    {
      throw new ForbiddenException($"User does not have access to workspace: {workspaceId}");
    }

    if (!await _columnRepository.ColumnExistsInWorkspaceAsync(columnId, workspaceId))
    {
      throw new NotFoundException("Column", columnId);
    }

    var maxPosition = await _projectTaskRepository.GetMaxPositionAsync(columnId);

    var task = new ProjectTask(
      workspaceId: workspaceId,
      columnId: columnId,
      title: createProjectTaskDto.Title,
      priority: createProjectTaskDto.Priority,
      position: maxPosition + 1,
      description: createProjectTaskDto.Description,
      dueDate: createProjectTaskDto.DueDate
    );

    var createdTask = await _projectTaskRepository.CreateAsync(task);
    return _mapper.Map<ProjectTaskDto>(createdTask);
  }

  public async Task<ProjectTaskDto> UpdateAsync(Guid taskId, UpdateProjectTaskDto updateProjectTaskDto, string userId)
  {
    var task = await _projectTaskRepository.GetByIdAsync(taskId);
    if (task == null) throw new NotFoundException("Task", taskId);

    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(task.WorkspaceId, userId))
    {
      throw new ForbiddenException($"User does not have access to task: {taskId}");
    }

    task.UpdateDetails(
      updateProjectTaskDto.Title,
      updateProjectTaskDto.Description,
      updateProjectTaskDto.DueDate,
      updateProjectTaskDto.Priority
    );

    task.UpdatePosition(updateProjectTaskDto.Position);

    var updatedTask = await _projectTaskRepository.UpdateAsync(task);
    return _mapper.Map<ProjectTaskDto>(updatedTask);
  }

  public async Task DeleteAsync(Guid taskId, string userId)
  {
    var task = await _projectTaskRepository.GetByIdAsync(taskId);
    if (task == null) throw new NotFoundException("Task", taskId);

    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(task.WorkspaceId, userId))
    {
      throw new ForbiddenException($"User does not have access to task: {taskId}");
    }

    await _projectTaskRepository.DeleteAsync(task);
  }

  public async Task<ProjectTaskDto> AddAssigneeToTaskAsync(Guid taskId, Guid assigneeId, string userId)
  {
    var task = await _projectTaskRepository.GetByIdAsync(taskId);
    if (task == null) throw new NotFoundException("Task", taskId);

    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(task.WorkspaceId, userId))
      throw new ForbiddenException($"User does not have access to task: {taskId}");

    var assignee = await _assigneeRepository.GetByIdAsync(assigneeId);
    if (assignee == null) throw new NotFoundException("Assignee", assigneeId);
    if (assignee.UserId != userId) throw new ForbiddenException($"User does not have access to assignee {assigneeId}");

    var addedAssigneeToTask = await _projectTaskRepository.AddAssigneeToTaskAsync(task, assignee);
    return _mapper.Map<ProjectTaskDto>(addedAssigneeToTask);
  }

  public async Task<ProjectTaskDto> RemoveAssigneeFromTaskAsync(Guid taskId, Guid assigneeId, string userId)
  {
    var task = await _projectTaskRepository.GetByIdAsync(taskId);
    if (task == null) throw new NotFoundException("Task", taskId);

    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(task.WorkspaceId, userId))
      throw new ForbiddenException($"User does not have access to task: {taskId}");

    var assignee = await _assigneeRepository.GetByIdAsync(assigneeId);
    if (assignee == null) throw new NotFoundException("Assignee", assigneeId);
    if (assignee.UserId != userId) throw new ForbiddenException($"User does not have access to assignee: {assigneeId}");

    var removedAssigneeFromTask = await _projectTaskRepository.RemoveAssigneeFromTaskAsync(task, assignee);
    return _mapper.Map<ProjectTaskDto>(removedAssigneeFromTask);
  }

  public async Task<ProjectTaskDto> AddTagToTaskAsync(Guid taskId, Guid tagId, string userId)
  {
    var task = await _projectTaskRepository.GetByIdAsync(taskId);
    if (task == null) throw new NotFoundException("Task", taskId);

    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(task.WorkspaceId, userId))
      throw new ForbiddenException($"User does not have access to task: {taskId}");

    var tag = await _tagRepository.GetByIdAsync(tagId);
    if (tag == null) throw new NotFoundException("Tag", tagId);
    if (tag.UserId != userId) throw new ForbiddenException($"User does not have access to tag: {tagId}");

    var addedTagToTask = await _projectTaskRepository.AddTagToTaskAsync(task, tag);
    return _mapper.Map<ProjectTaskDto>(addedTagToTask);
  }

  public async Task<ProjectTaskDto> RemoveTagFromTaskAsync(Guid taskId, Guid tagId, string userId)
  {
    var task = await _projectTaskRepository.GetByIdAsync(taskId);
    if (task == null) throw new NotFoundException("Task", taskId);

    if (!await _workspaceRepository.UserHasAccessToWorkspaceAsync(task.WorkspaceId, userId))
      throw new ForbiddenException($"User does not have access to task: {taskId}");

    var tag = await _tagRepository.GetByIdAsync(tagId);
    if (tag == null) throw new NotFoundException("Tag", tagId);
    if (tag.UserId != userId) throw new ForbiddenException($"User does not have access to tag: {tagId}");

    var removedTagFromTask = await _projectTaskRepository.RemoveTagFromTaskAsync(task, tag);
    return _mapper.Map<ProjectTaskDto>(removedTagFromTask);
  }
}