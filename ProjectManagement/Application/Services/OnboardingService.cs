using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectManagement.Domain;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Infrastructure.Data;
using ProjectManagement.Application.Interfaces.Services;


namespace ProjectManagement.Application.Services;

public class OnboardingService : IOnboardingService
{
  private readonly IAssigneeRepository _assigneeRepository;
  private readonly ITagRepository _tagRepository;
  private readonly IWorkspaceRepository _workspaceRepository;
  private readonly IColumnRepository _columnRepository;
  private readonly IProjectTaskRepository _projectTaskRepository;
  private readonly AppDbContext _dbContext;
  private readonly IMapper _mapper;
  private readonly ILogger<OnboardingService> _logger;

  public OnboardingService(
      IAssigneeRepository assigneeRepository,
      ITagRepository tagRepository,
      IWorkspaceRepository workspaceRepository,
      IColumnRepository columnRepository,
      IProjectTaskRepository projectTaskRepository,
      AppDbContext dbContext,
      IMapper mapper,
      ILogger<OnboardingService> logger)
  {
    _assigneeRepository = assigneeRepository;
    _tagRepository = tagRepository;
    _workspaceRepository = workspaceRepository;
    _columnRepository = columnRepository;
    _projectTaskRepository = projectTaskRepository;
    _dbContext = dbContext;
    _mapper = mapper;
    _logger = logger;
  }

  public async Task<(bool Success, string? Error)> CreateInitialDataAsync(string userId)
  {
    _logger.LogInformation("Starting onboarding process for user {UserId}", userId);

    var useTransaction = _dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory";
    IDbContextTransaction? transaction = null;

    if (useTransaction)
    {
      transaction = await _dbContext.Database.BeginTransactionAsync();
    }

    try
    {
      var assignees = await CreateAssigneesAsync(userId);

      var tags = await CreateTagsAsync(userId);

      var workspace1Result = await CreateEcommerceWorkspaceAsync(userId, assignees, tags);

      var workspace2Result = await CreateTaskManagementWorkspaceAsync(userId, assignees, tags);

      var workspace3Result = await CreateFinanceWorkspaceAsync(userId, assignees, tags);

      var workspaceIds = new[] { workspace1Result.WorkspaceId, workspace2Result.WorkspaceId, workspace3Result.WorkspaceId };

      var totalTasks = workspace1Result.TaskCount + workspace2Result.TaskCount + workspace3Result.TaskCount;
      var totalAssignments = workspace1Result.AssignmentCount + workspace2Result.AssignmentCount + workspace3Result.AssignmentCount;
      var totalTagRelations = workspace1Result.TagRelationCount + workspace2Result.TagRelationCount + workspace3Result.TagRelationCount;

      if (transaction != null)
      {
        await transaction.CommitAsync();
        _logger.LogInformation("Onboarding completed successfully for user {UserId}. Created {TotalTasks} tasks, {TotalAssignments} assignments, {TotalTagRelations} tag relations", userId, totalTasks, totalAssignments, totalTagRelations);
      }
      else
      {
        _logger.LogInformation("Onboarding completed successfully for user {UserId} (no transaction used). Created {TotalTasks} tasks, {TotalAssignments} assignments, {TotalTagRelations} tag relations", userId, totalTasks, totalAssignments, totalTagRelations);
      }

      return (true, null);
    }
    catch (Exception ex)
    {
      if (transaction != null)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Onboarding failed for user {UserId} - transaction rolled back", userId);
      }
      else
      {
        _logger.LogError(ex, "Onboarding failed for user {UserId}", userId);
      }

      return (false, ex.Message);
    }
  }

  private async Task<List<Assignee>> CreateAssigneesAsync(string userId)
  {
    _logger.LogInformation("Creating assignees for user {UserId}", userId);

    var assignees = new List<Assignee>
        {
            new Assignee("Mahdi Isse", userId, "/mahdi.jpg"),
            new Assignee("Emma van der Berg", userId, "/vrouw-4.jpg"),
            new Assignee("Lars Janssen", userId, "/man-1.jpg"),
            new Assignee("Sophie de Vries", userId, "/vrouw-1.jpg"),
            new Assignee("Tim Bakker", userId, "/man-3.jpg"),
            new Assignee("Lisa Meijer", userId, "/vrouw-2.jpg")
        };

    foreach (var assignee in assignees)
    {
      await _assigneeRepository.CreateAsync(assignee);
    }

    return assignees;
  }

  private async Task<List<Tag>> CreateTagsAsync(string userId)
  {
    _logger.LogInformation("Creating tags for user {UserId}", userId);

    var tags = new List<Tag>
        {
            new Tag("Frontend", "Blauw", userId),
            new Tag("Backend", "Groen", userId),
            new Tag("Database", "Paars", userId),
            new Tag("Bug Fix", "Rood", userId),
            new Tag("Feature", "Oranje", userId),
            new Tag("Design", "Roze", userId),
            new Tag("Testing", "Geel", userId),
            new Tag("DevOps", "Grijs", userId)
        };

    foreach (var tag in tags)
    {
      await _tagRepository.CreateAsync(tag);
    }

    return tags;
  }

  private async Task<(Guid WorkspaceId, int TaskCount, int AssignmentCount, int TagRelationCount)> CreateEcommerceWorkspaceAsync(
      string userId, List<Assignee> assignees, List<Tag> tags)
  {
    _logger.LogInformation("Creating E-commerce workspace for user {UserId}", userId);

    var workspace = new Workspace("E-commerce Platform", "Moderne webshop met React, Node.js en PostgreSQL", "Blauw", userId);
    workspace.SetPosition(0);
    var createdWorkspace = await _workspaceRepository.CreateAsync(workspace);

    var columns = new List<Column>();
    var columnNames = new[] { "To Do", "In Progress", "Review", "Done" };
    var columnColors = new[] { "border-blue-400", "border-yellow-400", "border-purple-400", "border-green-400" };

    for (int i = 0; i < columnNames.Length; i++)
    {
      var column = new Column(columnNames[i], columnColors[i], createdWorkspace.Id);
      column.SetPosition(i);
      var createdColumn = await _columnRepository.CreateAsync(column);
      columns.Add(createdColumn);
    }

    var tasks = new List<ProjectTask>();
    var taskData = new[]
    {
            new { Title = "Zoekfilters optimaliseren", Description = "Faceted search implementeren met Elasticsearch voor betere product discovery", Priority = TaskPriority.Medium, Position = 0 },
            new { Title = "Checkout flow A/B test", Description = "Conversion rate verbeteren door guest checkout vs account required te testen", Priority = TaskPriority.High, Position = 1 },
            new { Title = "Inventory management", Description = "Real-time voorraad tracking met low-stock waarschuwingen voor admins", Priority = TaskPriority.Medium, Position = 2 },

            new { Title = "Stripe webhook verwerking", Description = "Payment confirmed/failed events afhandelen en order status synchroniseren", Priority = TaskPriority.High, Position = 0 },
            new { Title = "Product review moderatie", Description = "Automated spam detection en admin approval workflow voor reviews", Priority = TaskPriority.Low, Position = 1 },

            new { Title = "Performance audit resultaten", Description = "Lighthouse score verbeteren: lazy loading, image optimization, code splitting", Priority = TaskPriority.High, Position = 0 },
            new { Title = "GDPR compliance check", Description = "Cookie consent, data retention policies en gebruiker data export functionaliteit", Priority = TaskPriority.Medium, Position = 1 },

            new { Title = "Abandoned cart recovery", Description = "Email automation voor onvoltooide bestellingen met discount codes", Priority = TaskPriority.Medium, Position = 0 },
            new { Title = "Social login integratie", Description = "Google en Facebook OAuth voor snellere account aanmaak", Priority = TaskPriority.Low, Position = 1 },
            new { Title = "Admin dashboard analytics", Description = "Sales metrics, top products en customer insights voor business team", Priority = TaskPriority.High, Position = 2 }
        };

    var columnIndex = 0;
    var positionInColumn = 0;
    foreach (var data in taskData)
    {
      if (positionInColumn >= 3) { columnIndex++; positionInColumn = 0; }
      if (columnIndex >= columns.Count) columnIndex = columns.Count - 1;

      var task = new ProjectTask(
          workspaceId: createdWorkspace.Id,
          columnId: columns[columnIndex].Id,
          title: data.Title,
          priority: data.Priority,
          position: positionInColumn,
          description: data.Description,
          dueDate: data.Position % 2 == 0 ? DateTime.UtcNow.AddDays(7 + data.Position) : null
      );

      var createdTask = await _projectTaskRepository.CreateAsync(task);
      tasks.Add(createdTask);

      positionInColumn++;
    }

    var assignmentCount = 0;
    var specificAssignments = new Dictionary<string, int[]>
    {
      ["Zoekfilters optimaliseren"] = new[] { 3, 4 },
      ["Checkout flow A/B test"] = new[] { 2, 6 },
      ["Inventory management"] = new[] { 1 },
      ["Stripe webhook verwerking"] = new[] { 3 },
      ["Product review moderatie"] = new[] { 4 },
      ["Performance audit resultaten"] = new[] { 5 },
      ["GDPR compliance check"] = new[] { 1 },
      ["Abandoned cart recovery"] = new[] { 3, 2 },
      ["Social login integratie"] = new[] { 4 },
      ["Admin dashboard analytics"] = new[] { 1, 6 }
    };

    foreach (var task in tasks)
    {
      if (specificAssignments.TryGetValue(task.Title, out var assigneeIndices))
      {
        foreach (var assigneeIndex in assigneeIndices)
        {
          if (assigneeIndex - 1 < assignees.Count)
          {
            var assignee = assignees[assigneeIndex - 1];
            await _projectTaskRepository.AddAssigneeToTaskAsync(task, assignee);
            assignmentCount++;
          }
        }
      }
    }

    var tagRelationCount = 0;
    var specificTagRelations = new Dictionary<string, int[]>
    {
      ["Zoekfilters optimaliseren"] = new[] { 2, 3, 5 },
      ["Checkout flow A/B test"] = new[] { 1, 6, 7 },
      ["Inventory management"] = new[] { 2, 3, 5 },
      ["Stripe webhook verwerking"] = new[] { 2, 5 },
      ["Product review moderatie"] = new[] { 2, 1, 5 },
      ["Performance audit resultaten"] = new[] { 1, 7, 8 },
      ["GDPR compliance check"] = new[] { 2, 7 },
      ["Abandoned cart recovery"] = new[] { 2, 1, 5 },
      ["Social login integratie"] = new[] { 2, 5 },
      ["Admin dashboard analytics"] = new[] { 1, 2, 6 }
    };

    foreach (var task in tasks)
    {
      if (specificTagRelations.TryGetValue(task.Title, out var tagIndices))
      {
        foreach (var tagIndex in tagIndices)
        {
          if (tagIndex - 1 < tags.Count)
          {
            var tag = tags[tagIndex - 1];
            await _projectTaskRepository.AddTagToTaskAsync(task, tag);
            tagRelationCount++;
          }
        }
      }
    }

    return (createdWorkspace.Id, tasks.Count, assignmentCount, tagRelationCount);
  }

  private async Task<(Guid WorkspaceId, int TaskCount, int AssignmentCount, int TagRelationCount)> CreateTaskManagementWorkspaceAsync(
      string userId, List<Assignee> assignees, List<Tag> tags)
  {
    _logger.LogInformation("Creating Task Management workspace for user {UserId}", userId);

    var workspace = new Workspace("Task Management App", "Portfolio project - Kanban board met drag & drop functionaliteit", "Groen", userId);
    workspace.SetPosition(1);
    var createdWorkspace = await _workspaceRepository.CreateAsync(workspace);

    var columns = new List<Column>();
    var columnNames = new[] { "Backlog", "In Development", "Testing", "Completed" };
    var columnColors = new[] { "border-gray-400", "border-blue-400", "border-orange-400", "border-green-400" };

    for (int i = 0; i < columnNames.Length; i++)
    {
      var column = new Column(columnNames[i], columnColors[i], createdWorkspace.Id);
      column.SetPosition(i);
      var createdColumn = await _columnRepository.CreateAsync(column);
      columns.Add(createdColumn);
    }

    var tasks = new List<ProjectTask>();
    var taskData = new[]
    {
      new { Title = "Bulk operations UI", Description = "Multiple taken tegelijk kunnen verplaatsen, assignen of verwijderen", Priority = TaskPriority.Low, Position = 0, ColumnIndex = 0 },
      new { Title = "Workspace templates", Description = "Pre-configured project templates (Scrum, Kanban, Bug Tracking)", Priority = TaskPriority.Low, Position = 1, ColumnIndex = 0 },

      new { Title = "Real-time collaboration cursor", Description = "WebSocket implementatie om live user cursors te tonen tijdens editing", Priority = TaskPriority.Medium, Position = 0, ColumnIndex = 1 },
      new { Title = "Advanced filtering system", Description = "Complex queries: assigned to me + high priority + due this week", Priority = TaskPriority.High, Position = 1, ColumnIndex = 1 },
      new { Title = "Keyboard shortcuts", Description = "Power user features: snelle navigatie en taak creation via hotkeys", Priority = TaskPriority.Low, Position = 2, ColumnIndex = 1 },

      new { Title = "Drag & drop edge cases", Description = "Cross-browser testing en touch device compatibility voor mobile users", Priority = TaskPriority.High, Position = 0, ColumnIndex = 2 },
      new { Title = "Load testing scenarios", Description = "Performance onder 1000+ concurrent users met realistic data volumes", Priority = TaskPriority.Medium, Position = 1, ColumnIndex = 2 },

      new { Title = "Workspace invitation flow", Description = "Email invites met role-based permissions (Admin, Member, Viewer)", Priority = TaskPriority.High, Position = 0, ColumnIndex = 3 },
      new { Title = "Comment threads systeem", Description = "Taak discussies met mentions, notifications en email digest", Priority = TaskPriority.Medium, Position = 1, ColumnIndex = 3 },
      new { Title = "Activity feed dashboard", Description = "Timeline van alle workspace changes voor project transparency", Priority = TaskPriority.Medium, Position = 2, ColumnIndex = 3 }
    };

    foreach (var data in taskData)
    {
      var task = new ProjectTask(
          workspaceId: createdWorkspace.Id,
          columnId: columns[data.ColumnIndex].Id,
          title: data.Title,
          priority: data.Priority,
          position: data.Position,
          description: data.Description,
          dueDate: data.Position % 2 == 0 ? DateTime.UtcNow.AddDays(5 + data.Position) : null
      );

      var createdTask = await _projectTaskRepository.CreateAsync(task);
      tasks.Add(createdTask);
    }

    var assignmentCount = 0;
    var specificAssignments = new Dictionary<string, int[]>
    {
      ["Bulk operations UI"] = new[] { 2 },
      ["Workspace templates"] = new[] { 4, 6 },
      ["Real-time collaboration cursor"] = new[] { 3, 2 },
      ["Advanced filtering system"] = new[] { 1, 4 },
      ["Keyboard shortcuts"] = new[] { 2 },
      ["Drag & drop edge cases"] = new[] { 5, 2 },
      ["Load testing scenarios"] = new[] { 5, 3 },
      ["Workspace invitation flow"] = new[] { 3, 4 },
      ["Comment threads systeem"] = new[] { 1, 2 },
      ["Activity feed dashboard"] = new[] { 4, 6 }
    };

    foreach (var task in tasks)
    {
      if (specificAssignments.TryGetValue(task.Title, out var assigneeIndices))
      {
        foreach (var assigneeIndex in assigneeIndices)
        {
          if (assigneeIndex - 1 < assignees.Count)
          {
            var assignee = assignees[assigneeIndex - 1];
            await _projectTaskRepository.AddAssigneeToTaskAsync(task, assignee);
            assignmentCount++;
          }
        }
      }
    }

    var tagRelationCount = 0;
    var specificTagRelations = new Dictionary<string, int[]>
    {
      ["Bulk operations UI"] = new[] { 1, 5 },
      ["Workspace templates"] = new[] { 1, 6, 5 },
      ["Real-time collaboration cursor"] = new[] { 2, 1, 5 },
      ["Advanced filtering system"] = new[] { 1, 2, 5 },
      ["Keyboard shortcuts"] = new[] { 1, 5 },
      ["Drag & drop edge cases"] = new[] { 1, 7, 4 },
      ["Load testing scenarios"] = new[] { 2, 7, 8 },
      ["Workspace invitation flow"] = new[] { 2, 1, 5 },
      ["Comment threads systeem"] = new[] { 2, 1, 5 },
      ["Activity feed dashboard"] = new[] { 1, 2, 6 }
    };

    foreach (var task in tasks)
    {
      if (specificTagRelations.TryGetValue(task.Title, out var tagIndices))
      {
        foreach (var tagIndex in tagIndices)
        {
          if (tagIndex - 1 < tags.Count)
          {
            var tag = tags[tagIndex - 1];
            await _projectTaskRepository.AddTagToTaskAsync(task, tag);
            tagRelationCount++;
          }
        }
      }
    }

    return (createdWorkspace.Id, tasks.Count, assignmentCount, tagRelationCount);
  }

  private async Task<(Guid WorkspaceId, int TaskCount, int AssignmentCount, int TagRelationCount)> CreateFinanceWorkspaceAsync(
      string userId, List<Assignee> assignees, List<Tag> tags)
  {
    _logger.LogInformation("Creating Personal Finance workspace for user {UserId}", userId);

    var workspace = new Workspace("Personal Finance Tracker", "Side project voor persoonlijk budget en uitgaven beheer", "Paars", userId);
    workspace.SetPosition(2);
    var createdWorkspace = await _workspaceRepository.CreateAsync(workspace);

    var columns = new List<Column>();
    var columnNames = new[] { "Planning", "Building", "Testing", "Deployed" };
    var columnColors = new[] { "border-indigo-400", "border-yellow-400", "border-red-400", "border-green-400" };

    for (int i = 0; i < columnNames.Length; i++)
    {
      var column = new Column(columnNames[i], columnColors[i], createdWorkspace.Id);
      column.SetPosition(i);
      var createdColumn = await _columnRepository.CreateAsync(column);
      columns.Add(createdColumn);
    }

    var tasks = new List<ProjectTask>();
    var taskData = new[]
    {
      new { Title = "Uitgaven categorisatie systeem", Description = "Automatische categorisering van transacties op basis van merchant namen en keywords", Priority = TaskPriority.Medium, Position = 0, ColumnIndex = 0 },
      new { Title = "Maandelijkse budget alerts", Description = "Email notificaties wanneer uitgaven 80% van budget bereiken per categorie", Priority = TaskPriority.High, Position = 1, ColumnIndex = 0 },

      new { Title = "Bank CSV import functie", Description = "Upload en parse bank statements van ING, ABN AMRO en Rabobank formaten", Priority = TaskPriority.High, Position = 0, ColumnIndex = 1 },
      new { Title = "Uitgaven dashboard charts", Description = "Interactive donut en line charts voor maandelijkse spending met Chart.js", Priority = TaskPriority.Medium, Position = 1, ColumnIndex = 1 },
      new { Title = "Recurring transactions tracker", Description = "Automatische herkenning van vaste lasten (Netflix, Spotify, huur) voor budgettering", Priority = TaskPriority.Low, Position = 2, ColumnIndex = 1 },

      new { Title = "CSV data validation", Description = "Error handling voor corrupte bestanden en onbekende transaction formaten", Priority = TaskPriority.High, Position = 0, ColumnIndex = 2 },
      new { Title = "Cross-browser compatibility", Description = "Responsive design testing en Safari/Firefox/Chrome compatibility checks", Priority = TaskPriority.Medium, Position = 1, ColumnIndex = 2 },

      new { Title = "Transaction CRUD operaties", Description = "Handmatige transacties toevoegen, bewerken en verwijderen met form validatie", Priority = TaskPriority.High, Position = 0, ColumnIndex = 3 },
      new { Title = "Export naar Excel functie", Description = "Filtered transaction data export voor belastingaangifte en accountant", Priority = TaskPriority.Medium, Position = 1, ColumnIndex = 3 },
      new { Title = "Savings goals tracker", Description = "Visual progress tracking voor spaar doelen met percentage en tijd indicatoren", Priority = TaskPriority.Medium, Position = 2, ColumnIndex = 3 }
    };

    foreach (var data in taskData)
    {
      var task = new ProjectTask(
          workspaceId: createdWorkspace.Id,
          columnId: columns[data.ColumnIndex].Id,
          title: data.Title,
          priority: data.Priority,
          position: data.Position,
          description: data.Description,
          dueDate: data.Position % 2 == 0 ? DateTime.UtcNow.AddDays(10 + data.Position) : null
      );

      var createdTask = await _projectTaskRepository.CreateAsync(task);
      tasks.Add(createdTask);
    }

    var assignmentCount = 0;
    var specificAssignments = new Dictionary<string, int[]>
    {
      ["Uitgaven categorisatie systeem"] = new[] { 3, 4 },
      ["Maandelijkse budget alerts"] = new[] { 1, 3 },
      ["Bank CSV import functie"] = new[] { 3, 1 },
      ["Uitgaven dashboard charts"] = new[] { 2, 6 },
      ["Recurring transactions tracker"] = new[] { 4 },
      ["CSV data validation"] = new[] { 5 },
      ["Cross-browser compatibility"] = new[] { 5, 2 },
      ["Transaction CRUD operaties"] = new[] { 1, 4 },
      ["Export naar Excel functie"] = new[] { 3, 5 },
      ["Savings goals tracker"] = new[] { 2, 6 }
    };

    foreach (var task in tasks)
    {
      if (specificAssignments.TryGetValue(task.Title, out var assigneeIndices))
      {
        foreach (var assigneeIndex in assigneeIndices)
        {
          if (assigneeIndex - 1 < assignees.Count)
          {
            var assignee = assignees[assigneeIndex - 1];
            await _projectTaskRepository.AddAssigneeToTaskAsync(task, assignee);
            assignmentCount++;
          }
        }
      }
    }

    var tagRelationCount = 0;
    var specificTagRelations = new Dictionary<string, int[]>
    {
      ["Uitgaven categorisatie systeem"] = new[] { 2, 3, 5 },
      ["Maandelijkse budget alerts"] = new[] { 2, 5 },
      ["Bank CSV import functie"] = new[] { 2, 5 },
      ["Uitgaven dashboard charts"] = new[] { 1, 6, 5 },
      ["Recurring transactions tracker"] = new[] { 2, 3, 5 },
      ["CSV data validation"] = new[] { 2, 7 },
      ["Cross-browser compatibility"] = new[] { 1, 7 },
      ["Transaction CRUD operaties"] = new[] { 1, 2, 5 },
      ["Export naar Excel functie"] = new[] { 2, 5 },
      ["Savings goals tracker"] = new[] { 1, 6, 5 }
    };

    foreach (var task in tasks)
    {
      if (specificTagRelations.TryGetValue(task.Title, out var tagIndices))
      {
        foreach (var tagIndex in tagIndices)
        {
          if (tagIndex - 1 < tags.Count)
          {
            var tag = tags[tagIndex - 1];
            await _projectTaskRepository.AddTagToTaskAsync(task, tag);
            tagRelationCount++;
          }
        }
      }
    }

    return (createdWorkspace.Id, tasks.Count, assignmentCount, tagRelationCount);
  }
}
