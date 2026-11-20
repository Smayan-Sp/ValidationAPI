using Microsoft.EntityFrameworkCore;
using Validation_Management_API.Data;
using Validation_Management_API.Models.DTO;
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
    }
}
