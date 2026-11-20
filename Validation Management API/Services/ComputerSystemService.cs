using Microsoft.EntityFrameworkCore;
using Validation_Management_API.Data;
using Validation_Management_API.Models;
using Validation_Management_API.Models.DTO;
using Validation_Management_API.Models.Enums;
using Validation_Management_API.Services.Interfaces;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Services
{
    public class ComputerSystemService : IComputerSystemService
    {
        private readonly ApplicationDbContext _db;

        public ComputerSystemService(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<ServiceResult<ComputerSystemDTO>> GetSystem(int id)
        {
            try
            {
                var dto = await _db.ComputerSystems
                    .AsNoTracking()
                    .Where(u => u.Id == id)
                    .Select(u => new ComputerSystemDTO
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Code = u.Code,
                        Owner = new IdNameDTO { Id = u.Owner.Id, Name = u.Owner.Name },
                        Location = new IdNameDTO { Id = u.Location.Id, Name = u.Location.Name },
                        Department = new IdNameDTO { Id = u.Department.Id, Name = u.Department.Name },
                        ValidationHistory = u.History!.Select(v => new ValidationHistoryDTO
                        {
                            Id = v.Id,
                            Status = v.Status,
                            ChangedAt = v.ChangedAt,
                            Project = v.Project == null ? null : v.Project.Name
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (dto == null)
                {
                    return ServiceResult<ComputerSystemDTO>.NotFound("System does not exist");
                }
                return ServiceResult<ComputerSystemDTO>.Ok(dto);
            }
            catch (Exception) 
            {
                return ServiceResult<ComputerSystemDTO>.InternalServerError($"Server failed to fetch your data");
            }
            
        }

        public async Task<ServiceResult<int>> CreateSystem(CreateSystemDTO dto)
        {

            if(!await _db.Departments.AnyAsync(u => u.Id == dto.DepartmentId))
            {
                return ServiceResult<int>.NotFound($"Department with id {dto.DepartmentId} does not exist");
            }

            if(!await _db.Locations.AnyAsync(u =>u.Id == dto.LocationId))
            {
                return ServiceResult<int>.NotFound($"Locaation with id {dto.LocationId} does not exist");
            }
            if (!await _db.Employees.AnyAsync(u => u.Id == dto.OwnerId))
            {
                return ServiceResult<int>.NotFound($"Owner/Employee with id {dto.OwnerId} does not exist");
            }

            ComputerSystem system = new ComputerSystem
            {
                Name = dto.Name,
                Code = dto.Code,
                DepartmentId = dto.DepartmentId,
                LocationId = dto.LocationId,
                OwnerId = dto.OwnerId
            };
            await _db.AddAsync(system);
            await _db.SaveChangesAsync();
            return ServiceResult<int>.CreatedAtAction("System successfully created", system.Id);
        }

        public async Task<ServiceResult<SystemSummaryDTO>> GetSystemSummary(int id)
        {
            var dto = await _db.ComputerSystems
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new SystemSummaryDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    TotalProjects = u.Projects!.Count(p => p.Status == Models.Enums.ProjectStatus.Completed),
                    CurrentlyActiveProjectCount = u.Projects!.Count(p => p.Status == Models.Enums.ProjectStatus.InProgress),
                    LatestValidationDate = u.History!
                        .Where(h => h.Status == Models.Enums.ValidationStatus.Completed)
                        .Max(h => (DateTime?)h.ChangedAt)
                }).FirstOrDefaultAsync();
            if (dto == null)
            {
                return ServiceResult<SystemSummaryDTO>.NotFound("Content does not exist");
            }
            return ServiceResult<SystemSummaryDTO>.Ok(dto);
        }

        public async Task<ServiceResult<List<SystemSearchResultDTO>>> SearchSystem(SearchSystemDTO dto)
        {
            IQueryable<ComputerSystem> query = _db.ComputerSystems.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                query = query.Where(s => s.Name.Contains(dto.Name));
            }
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                query = query.Where(s => s.Code.Contains(dto.Code));
            }
            if (dto.LocationId != null)
            {
                query = query.Where(s => s.LocationId == dto.LocationId);
            }
            if (dto.DepartmentId != null)
            {
                query = query.Where(s => s.DepartmentId == dto.DepartmentId);
            }
            if (dto.OwnerId != null)
            {
                query = query.Where(s => s.OwnerId == dto.OwnerId);
            }
            if (dto.ValidationStatus != null)
            {
                query = query.Where(s => s.History!
                    .OrderByDescending(h => h.ChangedAt)
                    .ThenByDescending(h => h.Id)
                    .Select(h => h.Status)
                    .FirstOrDefault() == dto.ValidationStatus);
            }
            if (dto.CreatedFrom != null)
            {
                query = query.Where(p => p.CreatedAt >= dto.CreatedFrom);
            }
            if (dto.CreatedTo != null)
            {
                query = query.Where(p => p.CreatedAt <= dto.CreatedTo);
            }

            query = query.Where(s => s.IsActive);

            IOrderedQueryable<ComputerSystem> ordered = (dto.SortBy, dto.Descending) switch
            {
                (SystemSortBy.Code, false) => query.OrderBy(s => s.Code),
                (SystemSortBy.Code, true) => query.OrderByDescending(s => s.Code),
                (SystemSortBy.CreatedAt, false) => query.OrderBy(s => s.CreatedAt),
                (SystemSortBy.CreatedAt, true) => query.OrderByDescending(s => s.CreatedAt),
                (_, true) => query.OrderByDescending(s => s.Name),
                _ => query.OrderBy(s => s.Name)
            };

            var result = await ordered.ThenBy(s => s.Id)
                .Select(s => new SystemSearchResultDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    DepartmentName = s.Department.Name,
                    LocationName = s.Location.Name,
                    OwnerName = s.Owner.Name,
                    ValidationStatus = s.History!
                    .OrderByDescending(h => h.ChangedAt)
                    .ThenByDescending(h => h.Id)
                    .Select(h => h.Status)
                    .FirstOrDefault(),
                    CreatedAt = s.CreatedAt
                }).ToListAsync();
            return ServiceResult<List<SystemSearchResultDTO>>.Ok(result);
        }

        public async Task<ServiceResult<bool>> DeactivateSystem(int id)
        {
            ComputerSystem? computerSystem = await _db.ComputerSystems
                .Where(s => s.Id == id).FirstOrDefaultAsync();

            if(computerSystem == null)
            {
                return ServiceResult<bool>.NotFound("That system does not exist");
            }

            if (await _db.Projects.AnyAsync(p => 
            p.Status == ProjectStatus.InProgress &&
            p.Systems.Any(s => s.Id == computerSystem.Id)))
                return ServiceResult<bool>.Conflict("This system has an active project and cannot be deactivated");

            if(computerSystem.IsActive == false)
            {
                return ServiceResult<bool>.BadRequest("That system is not active");
            }
            computerSystem.IsActive = false;
            await _db.SaveChangesAsync();
            return ServiceResult<bool>.NoContent();
        }

        public async Task<ServiceResult<bool>> RestoreSystem(int id)
        {
            ComputerSystem? computerSystem = await _db.ComputerSystems
                .Where(s => s.Id == id).FirstOrDefaultAsync();

            if (computerSystem == null)
            {
                return ServiceResult<bool>.NotFound("That system does not exist");
            }

            if (computerSystem.IsActive == true)
            {
                return ServiceResult<bool>.BadRequest("That system is already active");
            }
            computerSystem.IsActive = true;
            await _db.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }
    }
}
