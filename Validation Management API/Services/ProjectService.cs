using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Validation_Management_API.Data;
using Validation_Management_API.Models;
using Validation_Management_API.Models.DTO;
using Validation_Management_API.Models.Enums;
using Validation_Management_API.Services.Interfaces;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _db;
        public ProjectService (ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceResult<ProjectDTO>> GetProject(int id)
        {
            var dto = await _db.Projects
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new ProjectDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Locations = u.Locations.Select(l => new IdNameDTO { Id = l.Id, Name = l.Name }).ToList(),
                    Systems = u.Systems.Select(s => new IdNameDTO { Id = s.Id, Name = s.Name }).ToList(),
                    Manager = new IdNameDTO { Id = u.Manager.Id, Name = u.Manager.Name },
                    Status = u.Status,
                    PeopleInvolved = u.ProjectDocuments
                    .SelectMany(d => d.Tasks
                    .SelectMany(t => t.Contributors
                    .Select(c => new ProjectContributorDTO{
                        EmployeeId = c.Employee.Id, 
                        Name = c.Employee.Name, 
                        Role = t.Type,
                        DocumentName = d.DocumentList.Name})))
                    .OrderBy(p => p.DocumentName)
                    .ToList()
                }).FirstOrDefaultAsync();   

            if (dto == null)
            {
                return ServiceResult<ProjectDTO>.NotFound("The project does not exist");
            }
            return ServiceResult<ProjectDTO>.Ok(dto);

        }


        public async Task<ServiceResult<int>>CreateProject(CreateProjectDTO dto)
        {
            if (dto.PlannedStartDate.Kind != DateTimeKind.Utc || dto.PlannedEndDate.Kind != DateTimeKind.Utc)
            {
                return ServiceResult<int>.BadRequest("Planned dates must be in UTC (for example 2026-12-01T00:00:00Z)");
            }

            if (dto.PlannedEndDate < dto.PlannedStartDate)
            {
                return ServiceResult<int>.BadRequest("Planned end date cannot be before the planned start date");
            }

            var manager = await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.ManagerId);
            if (manager == null)
            {
                return ServiceResult<int>.NotFound($"Manager/Employee with id {dto.ManagerId} does not exist");
            }

            if (!manager.IsActive)
            {
                return ServiceResult<int>.BadRequest($"Manager/Employee with id {dto.ManagerId} is inactive");
            }

            List<ComputerSystem> ProjectSystems = await _db.ComputerSystems
                .Where(u => dto.SystemIds.Contains(u.Id)).ToListAsync();

            List<Location> ProjectLocations = await _db.Locations
                .Where(u => dto.LocationIds.Contains(u.Id)).ToListAsync();

            var missingSystemIds = dto.SystemIds.Distinct()
                .Except(ProjectSystems.Select(s => s.Id)).ToList();
            if (missingSystemIds.Count > 0)
            {
                return ServiceResult<int>.NotFound($"System ids {string.Join(", ", missingSystemIds)} do not exist");
            }

            var inactiveSystemIds = ProjectSystems.Where(s => !s.IsActive).Select(s => s.Id).ToList();
            if (inactiveSystemIds.Count > 0)
            {
                return ServiceResult<int>.BadRequest($"System ids {string.Join(", ", inactiveSystemIds)} are inactive");
            }

            var missingLocationIds = dto.LocationIds.Distinct()
                .Except(ProjectLocations.Select(l => l.Id)).ToList();
            if (missingLocationIds.Count > 0)
            {
                return ServiceResult<int>.NotFound($"Location ids {string.Join(", ", missingLocationIds)} do not exist");
            }


            Project project = new Project
            {
                Name = dto.Name,
                ManagerId = dto.ManagerId,
                ValidationType = dto.ValidationType,
                PlannedStartDate = dto.PlannedStartDate,
                PlannedEndDate = dto.PlannedEndDate,
                Systems = ProjectSystems,
                Locations = ProjectLocations
            };

            await _db.AddAsync(project);
            await _db.SaveChangesAsync();
            return ServiceResult<int>.CreatedAtAction("Project successfully created with id", project.Id);
        }

        public async Task<ServiceResult<int>> CloneProject(int sourceProjectId, CloneProjectDTO dto)
        {
            if (dto.PlannedStartDate.Kind != DateTimeKind.Utc || dto.PlannedEndDate.Kind != DateTimeKind.Utc)
            {
                return ServiceResult<int>.BadRequest("Planned dates must be in UTC (for example 2026-12-01T00:00:00Z)");
            }

            if (dto.PlannedEndDate < dto.PlannedStartDate)
            {
                return ServiceResult<int>.BadRequest("Planned end date cannot be before the planned start date");
            }

            var manager = await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.ManagerId);
            if (manager == null)
            {
                return ServiceResult<int>.NotFound($"Manager/Employee with id {dto.ManagerId} does not exist");
            }

            if (!manager.IsActive)
            {
                return ServiceResult<int>.BadRequest($"Manager/Employee with id {dto.ManagerId} is inactive");
            }

            var source = await _db.Projects
                .Include(p => p.Systems)
                .Include(p => p.Locations)
                .Include(p => p.ProjectDocuments).ThenInclude(d => d.Tasks)
                .Include(p => p.ProjectDocuments).ThenInclude(d => d.DependsOn)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == sourceProjectId);

            if (source == null)
            {
                return ServiceResult<int>.NotFound("The project to clone does not exist");
            }

            var inactiveSystemIds = source.Systems.Where(s => !s.IsActive).Select(s => s.Id).ToList();
            if (inactiveSystemIds.Count > 0)
            {
                return ServiceResult<int>.BadRequest($"System ids {string.Join(", ", inactiveSystemIds)} are inactive");
            }

            var shift = dto.PlannedStartDate - source.PlannedStartDate;

            var project = new Project
            {
                Name = dto.Name,
                ManagerId = dto.ManagerId,
                ValidationType = source.ValidationType,
                PlannedStartDate = dto.PlannedStartDate,
                PlannedEndDate = dto.PlannedEndDate,
                Status = ProjectStatus.Planned,
                Systems = source.Systems.ToList(),
                Locations = source.Locations.ToList()
            };

            var copies = new Dictionary<int, ProjectDocument>();

            foreach (var document in source.ProjectDocuments)
            {
                var copy = new ProjectDocument
                {
                    DocumentListId = document.DocumentListId,
                    PlannedStartDate = document.PlannedStartDate + shift,
                    PlannedEndDate = document.PlannedEndDate + shift,
                    Tasks = document.Tasks.Select(t => new ProjectTask
                    {
                        Type = t.Type,
                        Number = t.Number,
                        PlannedStartDate = t.PlannedStartDate + shift,
                        PlannedEndDate = t.PlannedEndDate + shift,
                        Status = ProjectTaskStatus.NotStarted,
                        IsOverdue = false
                    }).ToList()
                };

                copies[document.Id] = copy;
                project.ProjectDocuments.Add(copy);
            }

            foreach (var document in source.ProjectDocuments)
            {
                foreach (var dependency in document.DependsOn)
                {
                    if (copies.TryGetValue(dependency.Id, out var target))
                    {
                        copies[document.Id].DependsOn.Add(target);
                    }
                }
            }

            await _db.AddAsync(project);
            await _db.SaveChangesAsync();
            return ServiceResult<int>.CreatedAtAction("Project successfully cloned with id", project.Id);
        }

        public async Task<ServiceResult<List<ProjectSummaryDTO>>>SearchProject(SearchProjectDTO dto)
        {
            IQueryable<Project> query = _db.Projects.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                query = query.Where(p => p.Name.Contains(dto.Name));
            }
            if (dto.Status != null)
            {
                query = query.Where(p => p.Status == dto.Status);
            }
            if (dto.SystemId != null)
            {
                query = query.Where(p => p.Systems.Any(s => s.Id == dto.SystemId));
            }
            if (dto.LocationId != null)
            {
                query = query.Where(p => p.Locations.Any(l => l.Id == dto.LocationId));
            }
            if (dto.ManagerId != null)
            {
                query = query.Where(p => p.ManagerId == dto.ManagerId);
            }
            if (dto.ValidationType != null)
            {
                query = query.Where(p => p.ValidationType == dto.ValidationType);
            }
            if (dto.PlannedFrom != null)
            {
                query = query.Where(p => p.PlannedEndDate >= dto.PlannedFrom);
            }
            if (dto.PlannedTo != null)
            {
                query = query.Where(p => p.PlannedStartDate <= dto.PlannedTo);
            }
            if (dto.CreatedFrom != null)
            {
                query = query.Where(p => p.CreatedAt >= dto.CreatedFrom);
            }
            if (dto.CreatedTo != null)
            {
                query = query.Where(p => p.CreatedAt <= dto.CreatedTo);
            }

             
            var result = await query
                .OrderBy(u => u.Name)
                .Select(p => new ProjectSummaryDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status,
                    ValidationType = p.ValidationType,
                    Manager = new IdNameDTO { Id = p.ManagerId, Name = p.Manager.Name },
                    PlannedStartDate = p.PlannedStartDate,
                    PlannedEndDate = p.PlannedEndDate,
                    CreatedAt = p.CreatedAt,
                    Systems = p.Systems.Select(s => new IdNameDTO { Id = s.Id, Name = s.Name }).ToList(),
                    Locations = p.Locations.Select(l => new IdNameDTO { Id = l.Id, Name = l.Name }).ToList()
                }).ToListAsync();

            return ServiceResult<List<ProjectSummaryDTO>>.Ok(result);
        }

        public async Task<ServiceResult<DashboardDTO>> GetDashboard(int id)
        {
            var data = await _db.Projects
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    DocumentCount = p.ProjectDocuments.Count(),
                    NotStarted = p.ProjectDocuments.SelectMany(d => d.Tasks).Count(t => t.Status == ProjectTaskStatus.NotStarted),
                    InProgress = p.ProjectDocuments.SelectMany(d => d.Tasks).Count(t => t.Status == ProjectTaskStatus.InProgress),
                    Completed = p.ProjectDocuments.SelectMany(d => d.Tasks).Count(t => t.Status == ProjectTaskStatus.Completed),
                    Overdue = p.ProjectDocuments.SelectMany(d => d.Tasks).Count(t => t.IsOverdue)
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return ServiceResult<DashboardDTO>.NotFound("The project does not exist");

            var totalTasks = data.NotStarted + data.InProgress + data.Completed;
            var percentComplete = totalTasks == 0 ? 0m : Math.Round(data.Completed * 100m / totalTasks, 2);

            var dashboard = new DashboardDTO
            {
                ProjectId = data.Id,
                ProjectName = data.Name,
                DocumentCount = data.DocumentCount,
                TaskCounts = new TaskCountsDTO
                {
                    NotStarted = data.NotStarted,
                    InProgress = data.InProgress,
                    Completed = data.Completed
                },
                OverdueTaskCount = data.Overdue,
                PercentComplete = percentComplete
            };

            return ServiceResult<DashboardDTO>.Ok(dashboard);
        }

        public async Task<ServiceResult<bool>> ChangeStatus(int id, ProjectStatus status)
        {
            Project? project = await _db.Projects.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (project == null)
            {
                return ServiceResult<bool>.NotFound("That project does not exist");
            }

            bool allowed = (project.Status, status) switch
            {
                (ProjectStatus.Planned, ProjectStatus.InProgress) => true,
                (ProjectStatus.Planned, ProjectStatus.Cancelled) => true,
                (ProjectStatus.InProgress, ProjectStatus.Completed) => true,
                (ProjectStatus.InProgress, ProjectStatus.Cancelled) => true,
                _ => false
            };

            if (!allowed)
            {
                return ServiceResult<bool>.Conflict($"Cannot change status from {project.Status} to {status}");
            }

            if (status == ProjectStatus.Completed)
            {
                bool hasOpenTasks = await _db.ProjectTasks.AnyAsync(t =>
                    t.ProjectDocument.ProjectId == id &&
                    t.Status != ProjectTaskStatus.Completed);

                if (hasOpenTasks)
                {
                    return ServiceResult<bool>.Conflict("The project cannot be completed while it has open tasks");
                }

                project.CompletedAt = DateTime.UtcNow;
            }

            project.Status = status;
            await _db.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }
    }
}
