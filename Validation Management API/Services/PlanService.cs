using Microsoft.EntityFrameworkCore;
using Validation_Management_API.Data;
using Validation_Management_API.Models;
using Validation_Management_API.Models.DTO;
using Validation_Management_API.Models.Enums;
using Validation_Management_API.Services.Interfaces;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Services
{
    public class PlanService : IPlanService
    {
        private readonly ApplicationDbContext _db;

        public PlanService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceResult<PlanDTO>> GetPlan(int projectId)
        {
            var project = await _db.Projects
                .AsNoTracking()
                .Where(u => u.Id == projectId)
                .Select(p => new PlanDTO
                {
                    ProjectName = p.Name,
                    ProjectId = p.Id,
                    Documents = p.ProjectDocuments
                    .OrderBy(u => u.Id)
                    .Select(d => new PlanDocumentDTO
                    {
                        Id = d.Id,
                        Name = d.DocumentList.Name,
                        PlannedStartDate = d.PlannedStartDate,
                        PlannedEndDate = d.PlannedEndDate,
                        DependsOn = d.DependsOn
                        .OrderBy(o => o.Id)
                        .Select(de => new IdNameDTO { Id = de.Id, Name = de.DocumentList.Name }).ToList(),
                        Tasks = d.Tasks
                        .OrderBy(o => o.Id)
                        .Select(t => new PlanTaskDTO
                        {
                            Id = t.Id,
                            Type = t.Type,
                            Number = t.Number,
                            PlannedStartDate = t.PlannedStartDate,
                            PlannedEndDate = t.PlannedEndDate,
                            Status = t.Status,
                            Contributors = t.Contributors
                            .OrderBy(o => o.Id)
                            .Select(c => new IdNameDTO { Id = c.EmployeeId, Name = c.Employee.Name }).ToList()
                        }).ToList()
                    }).ToList(),
                }).FirstOrDefaultAsync();

            if (project == null)
                return ServiceResult<PlanDTO>.NotFound("The project does not exist");

            return ServiceResult<PlanDTO>.Ok(project);
        }

        public async Task<ServiceResult<AddPlanDocumentsResultDTO>> AddPlanDocuments(int projectId, List<AddPlanDocumentDTO> documents)
        {
            if (documents.Count == 0)
            {
                return ServiceResult<AddPlanDocumentsResultDTO>.BadRequest("Please send at least one document");
            }

            if (documents.Any(d => d.PlannedStartDate != null && d.PlannedStartDate.Value.Kind != DateTimeKind.Utc))
            {
                return ServiceResult<AddPlanDocumentsResultDTO>.BadRequest("Planned start dates must be in UTC (for example 2026-12-01T00:00:00Z)");
            }

            Project? project = await _db.Projects
                .Include(p => p.ProjectDocuments).ThenInclude(d => d.DocumentList).ThenInclude(l => l.DependsOn)
                .Include(p => p.ProjectDocuments).ThenInclude(d => d.DependsOn)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return ServiceResult<AddPlanDocumentsResultDTO>.NotFound("The project does not exist");
            }

            List<int> requestedIds = documents.Select(d => d.DocumentListId).Distinct().ToList();

            List<DocumentList> catalog = await _db.DocumentLists
                .Include(d => d.DependsOn)
                .Where(d => requestedIds.Contains(d.Id))
                .ToListAsync();

            var missingIds = requestedIds.Except(catalog.Select(c => c.Id)).ToList();
            if (missingIds.Count > 0)
            {
                return ServiceResult<AddPlanDocumentsResultDTO>.NotFound($"Document ids {string.Join(", ", missingIds)} do not exist");
            }

            var result = new AddPlanDocumentsResultDTO();
            var newDocuments = new List<ProjectDocument>();

            foreach (int requestedId in requestedIds)
            {
                DocumentList template = catalog.First(c => c.Id == requestedId);
                var idName = new IdNameDTO { Id = template.Id, Name = template.Name };

                if (project.ProjectDocuments.Any(d => d.DocumentListId == template.Id))
                {
                    result.Skipped.Add(idName);
                    continue;
                }

                DateTime start = documents.First(d => d.DocumentListId == requestedId).PlannedStartDate
                    ?? project.PlannedStartDate;

                var document = new ProjectDocument
                {
                    DocumentList = template,
                    PlannedStartDate = start,
                    PlannedEndDate = start.AddDays(template.Duration)
                };

                int stageDays = Math.Max(1, template.Duration / 3);

                void AddTasks(TaskType type, int count, int stageIndex)
                {
                    for (int number = 1; number <= count; number++)
                    {
                        document.Tasks.Add(new ProjectTask
                        {
                            Type = type,
                            Number = number,
                            PlannedStartDate = start.AddDays(stageDays * stageIndex),
                            PlannedEndDate = stageIndex == 2
                                ? document.PlannedEndDate
                                : start.AddDays(stageDays * (stageIndex + 1))
                        });
                    }
                }

                AddTasks(TaskType.Author, template.AuthorCount, 0);
                AddTasks(TaskType.Reviewer, template.ReviewerCount, 1);
                AddTasks(TaskType.Approver, template.ApproverCount, 2);

                project.ProjectDocuments.Add(document);
                newDocuments.Add(document);
                result.Added.Add(idName);
            }

            foreach (ProjectDocument newDocument in newDocuments)
            {
                foreach (ProjectDocument other in project.ProjectDocuments.Where(d => d != newDocument))
                {
                    bool newNeedsOther = newDocument.DocumentList.DependsOn.Any(r => r.Id == other.DocumentListId);
                    if (newNeedsOther && !newDocument.DependsOn.Contains(other))
                    {
                        newDocument.DependsOn.Add(other);
                    }

                    bool otherNeedsNew = other.DocumentList.DependsOn.Any(r => r.Id == newDocument.DocumentListId);
                    if (otherNeedsNew && !other.DependsOn.Contains(newDocument))
                    {
                        other.DependsOn.Add(newDocument);
                    }
                }
            }

            await _db.SaveChangesAsync();
            return ServiceResult<AddPlanDocumentsResultDTO>.Ok(result);
        }


        public async Task<ServiceResult<bool>> RemovePlanDocuments(int projectId, List<int> planDocumentIds)
        {
            if (planDocumentIds.Count == 0)
            {
                return ServiceResult<bool>.BadRequest("Please send at least one document id");
            }

            planDocumentIds = planDocumentIds.Distinct().ToList();

            if (!await _db.Projects.AnyAsync(p => p.Id == projectId))
            {
                return ServiceResult<bool>.NotFound("This project does not exist");
            }

            List<ProjectDocument> inProject = await _db.ProjectDocuments
                .Where(d => d.ProjectId == projectId && planDocumentIds.Contains(d.Id))
                .ToListAsync();

            List<int> notInProject = planDocumentIds.Except(inProject.Select(u => u.Id)).ToList();


            if (notInProject.Count > 0)
            {
                return ServiceResult<bool>.NotFound($"Documents with Ids {string.Join(", ", notInProject)} do not exist in this project");
            }

            _db.ProjectDocuments.RemoveRange(inProject);
            await _db.SaveChangesAsync();
            return ServiceResult<bool>.NoContent();

        }

        public async Task<ServiceResult<bool>> UpdateTask(int projectId, int taskId, UpdateTaskDTO dto)
        {
            if ((dto.PlannedStartDate != null && dto.PlannedStartDate.Value.Kind != DateTimeKind.Utc)
                || (dto.PlannedEndDate != null && dto.PlannedEndDate.Value.Kind != DateTimeKind.Utc))
            {
                return ServiceResult<bool>.BadRequest("Dates must be in UTC (for example 2026-12-01T00:00:00Z)");
            }

            ProjectTask? task = await _db.ProjectTasks
                .Include(t => t.ProjectDocument).ThenInclude(d => d.Tasks)
                .Include(t => t.Contributors)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectDocument.ProjectId == projectId);

            if (task == null)
            {
                return ServiceResult<bool>.NotFound("That task does not exist in this project");
            }

            ProjectDocument document = task.ProjectDocument;
            bool anyTaskStarted = document.Tasks.Any(t => t.Status != ProjectTaskStatus.NotStarted);

            DateTime start = dto.PlannedStartDate ?? task.PlannedStartDate;
            DateTime end = dto.PlannedEndDate ?? task.PlannedEndDate;

            if (end < start)
            {
                return ServiceResult<bool>.BadRequest("The end date can't be before the start date");
            }

            bool outsideDocument = start < document.PlannedStartDate || end > document.PlannedEndDate;

            if (outsideDocument)
            {
                if (anyTaskStarted)
                {
                    return ServiceResult<bool>.Conflict("The document has started tasks, so its planned dates are locked and the task must stay inside them");
                }

                if (start < document.PlannedStartDate)
                    document.PlannedStartDate = start;

                if (end > document.PlannedEndDate)
                    document.PlannedEndDate = end;
            }

            task.PlannedStartDate = start;
            task.PlannedEndDate = end;

            if (dto.Status != null && dto.Status != task.Status)
            {
                bool validMove = (task.Status, dto.Status.Value) switch
                {
                    (ProjectTaskStatus.NotStarted, ProjectTaskStatus.InProgress) => true,
                    (ProjectTaskStatus.InProgress, ProjectTaskStatus.Completed) => true,
                    _ => false
                };

                if (!validMove)
                {
                    return ServiceResult<bool>.BadRequest($"A task can't move from {task.Status} to {dto.Status}");
                }

                if (dto.Status == ProjectTaskStatus.InProgress)
                {
                    Project project = await _db.Projects.FirstAsync(p => p.Id == projectId);

                    if (project.Status == ProjectStatus.Planned)
                    {
                        project.Status = ProjectStatus.InProgress;
                    }
                }

                task.Status = dto.Status.Value;
            }

            if (dto.Contributors != null)
            {
                List<int> requestedIds = dto.Contributors.Select(c => c.EmployeeId).ToList();

                if (requestedIds.Count != requestedIds.Distinct().Count())
                {
                    return ServiceResult<bool>.BadRequest("A person can't be on the same task twice");
                }

                List<Employee> employees = await _db.Employees
                    .Where(e => requestedIds.Contains(e.Id))
                    .ToListAsync();

                List<int> missingIds = requestedIds.Except(employees.Select(e => e.Id)).ToList();

                if (missingIds.Count > 0)
                {
                    return ServiceResult<bool>.NotFound($"Employees with Ids {string.Join(", ", missingIds)} do not exist");
                }

                List<int> currentIds = task.Contributors.Select(c => c.EmployeeId).ToList();

                List<int> inactiveNew = employees
                    .Where(e => !e.IsActive && !currentIds.Contains(e.Id))
                    .Select(e => e.Id)
                    .ToList();

                if (inactiveNew.Count > 0)
                {
                    return ServiceResult<bool>.BadRequest($"Employees with Ids {string.Join(", ", inactiveNew)} are deactivated and can't be added");
                }

                task.Contributors.RemoveAll(c => !requestedIds.Contains(c.EmployeeId));

                foreach (TaskContributorInputDTO input in dto.Contributors)
                {
                    Contributor? existing = task.Contributors.FirstOrDefault(c => c.EmployeeId == input.EmployeeId);

                    if (existing != null)
                    {
                        existing.IsMandatory = input.IsMandatory;
                    }
                    else
                    {
                        task.Contributors.Add(new Contributor
                        {
                            EmployeeId = input.EmployeeId,
                            IsMandatory = input.IsMandatory
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();
            return ServiceResult<bool>.NoContent();
        }

        public async Task<ServiceResult<bool>> AddContributor(int projectId, int taskId, TaskContributorInputDTO dto)
        {
            ProjectTask? task = await _db.ProjectTasks
                .Include(t => t.Contributors)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectDocument.ProjectId == projectId);

            if (task == null)
            {
                return ServiceResult<bool>.NotFound("That task does not exist in this project");
            }

            Employee? employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);

            if (employee == null)
            {
                return ServiceResult<bool>.NotFound("That employee does not exist");
            }

            if (!employee.IsActive)
            {
                return ServiceResult<bool>.BadRequest("Deactivated employees can't be added to a task");
            }

            if (task.Contributors.Any(c => c.EmployeeId == dto.EmployeeId))
            {
                return ServiceResult<bool>.Conflict("That person is already on this task");
            }

            task.Contributors.Add(new Contributor
            {
                EmployeeId = dto.EmployeeId,
                IsMandatory = dto.IsMandatory
            });

            await _db.SaveChangesAsync();
            return ServiceResult<bool>.NoContent();
        }

        public async Task<ServiceResult<bool>> ReplaceContributor(int projectId, int taskId, int employeeId, TaskContributorInputDTO dto)
        {
            ProjectTask? task = await _db.ProjectTasks
                .Include(t => t.Contributors)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectDocument.ProjectId == projectId);

            if (task == null)
            {
                return ServiceResult<bool>.NotFound("That task does not exist in this project");
            }

            Contributor? existing = task.Contributors.FirstOrDefault(c => c.EmployeeId == employeeId);

            if (existing == null)
            {
                return ServiceResult<bool>.NotFound("That person is not on this task");
            }

            if (dto.EmployeeId != employeeId)
            {
                Employee? replacement = await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);

                if (replacement == null)
                {
                    return ServiceResult<bool>.NotFound("The replacement employee does not exist");
                }

                if (!replacement.IsActive)
                {
                    return ServiceResult<bool>.BadRequest("Deactivated employees can't be added to a task");
                }

                if (task.Contributors.Any(c => c.EmployeeId == dto.EmployeeId))
                {
                    return ServiceResult<bool>.Conflict("The replacement is already on this task");
                }

                existing.EmployeeId = dto.EmployeeId;
            }

            existing.IsMandatory = dto.IsMandatory;

            await _db.SaveChangesAsync();
            return ServiceResult<bool>.NoContent();
        }

        public async Task<ServiceResult<bool>> RemoveContributor(int projectId, int taskId, int employeeId)
        {
            Contributor? contributor = await _db.Contributors
                .FirstOrDefaultAsync(c => c.ProjectTaskId == taskId
                    && c.EmployeeId == employeeId
                    && c.ProjectTask.ProjectDocument.ProjectId == projectId);

            if (contributor == null)
            {
                return ServiceResult<bool>.NotFound("That person is not on this task");
            }

            _db.Contributors.Remove(contributor);
            await _db.SaveChangesAsync();
            return ServiceResult<bool>.NoContent();
        }

        public async Task<ServiceResult<bool>> RemoveTask(int projectId, int taskId)
        {
            ProjectTask? task = await _db.ProjectTasks
                .Include(t => t.ProjectDocument).ThenInclude(d => d.Tasks)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectDocument.ProjectId == projectId);

            if (task == null)
            {
                return ServiceResult<bool>.NotFound("That task does not exist in this project");
            }

            if (task.Status != ProjectTaskStatus.NotStarted)
            {
                return ServiceResult<bool>.Conflict("A task that has started can't be removed");
            }

            List<ProjectTask> sameKind = task.ProjectDocument.Tasks
                .Where(t => t.Type == task.Type && t.Id != task.Id)
                .OrderBy(t => t.Number)
                .ToList();

            await using var transaction = await _db.Database.BeginTransactionAsync();

            _db.ProjectTasks.Remove(task);
            await _db.SaveChangesAsync();

            for (int i = 0; i < sameKind.Count; i++)
            {
                sameKind[i].Number = i + 1;
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<bool>.NoContent();
        }
    }
}
