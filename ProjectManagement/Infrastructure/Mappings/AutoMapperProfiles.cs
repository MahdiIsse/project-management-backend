using AutoMapper;
using ProjectManagement.Domain;
using ProjectManagement.Application.DTOs.Assignees;
using ProjectManagement.Application.DTOs.Columns;
using ProjectManagement.Application.DTOs.ProjectTasks;
using ProjectManagement.Application.DTOs.Tags;
using ProjectManagement.Application.DTOs.Workspaces;

namespace ProjectManagement.Infrastructure.Mappings;

public class AutoMapperProfiles : Profile
{
  public AutoMapperProfiles()
  {
    CreateMap<Workspace, WorkspaceDto>()
      .ReverseMap();

    CreateMap<Column, ColumnDto>()
      .ReverseMap();

    CreateMap<ProjectTask, ProjectTaskDto>()
      .ReverseMap();

    CreateMap<Tag, TagDto>()
      .ReverseMap();

    CreateMap<Assignee, AssigneeDto>()
      .ReverseMap();
  }
}