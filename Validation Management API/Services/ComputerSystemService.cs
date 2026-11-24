using Microsoft.EntityFrameworkCore;
using Validation_Management_API.Data;
using Validation_Management_API.Helpers;
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

        public async Task<ServiceResult<int>> CreateSystem(CreateSystemDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return ServiceResult<int>.BadRequest("Name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                return ServiceResult<int>.BadRequest("Code cannot be empty");
            }

            if (!await _db.Departments.AnyAsync(u => u.Id == dto.DepartmentId))
            {
                return ServiceResult<int>.NotFound($"Department with id {dto.DepartmentId} does not exist");
            }

            if (!await _db.Locations.AnyAsync(u => u.Id == dto.LocationId))
            {
                return ServiceResult<int>.NotFound($"Location with id {dto.LocationId} does not exist");
            }

            Employee? owner = await _db.Employees.FirstOrDefaultAsync(u => u.Id == dto.OwnerId);
            if (owner == null)
            {
                return ServiceResult<int>.NotFound($"Owner/Employee with id {dto.OwnerId} does not exist");
            }

            if (!owner.IsActive)
            {
                return ServiceResult<int>.BadRequest($"Owner/Employee with id {dto.OwnerId} is inactive");
            }

            if (await _db.ComputerSystems.AnyAsync(s => s.Name == dto.Name))
            {
                return ServiceResult<int>.Conflict($"A system named {dto.Name} already exists");
            }

            if (await _db.ComputerSystems.AnyAsync(s => s.Code == dto.Code))
            {
                return ServiceResult<int>.Conflict($"A system with code {dto.Code} already exists");
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
                    TotalProjects = u.Projects!.Count(),
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

        public async Task<ServiceResult<PagedResult<SystemSearchResultDTO>>> SearchSystem(SearchSystemDTO dto)
        {
            IQueryable<ComputerSystem> query = _db.ComputerSystems.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                query = query.Where(s => EF.Functions.ILike(s.Name, $"%{dto.Name}%"));
            }
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                query = query.Where(s => EF.Functions.ILike(s.Code, $"%{dto.Code}%"));
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
                .ToPagedResultAsync(dto, s => new SystemSearchResultDTO
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
                });

            return ServiceResult<PagedResult<SystemSearchResultDTO>>.Ok(result);
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
            return ServiceResult<bool>.NoContent();
        }

        private const long MaxAttachmentBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedAttachmentExtensions =
            [".pdf", ".png", ".jpg", ".jpeg", ".txt", ".docx", ".xlsx"];

        public async Task<ServiceResult<int>> UploadAttachment(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return ServiceResult<int>.BadRequest("Please attach a non-empty file");
            }

            if (file.Length > MaxAttachmentBytes)
            {
                return ServiceResult<int>.BadRequest("The file must be 5 MB or smaller");
            }

            string fileName = Path.GetFileName(file.FileName);

            if (fileName.Length > 255)
            {
                return ServiceResult<int>.BadRequest("The file name must be 255 characters or fewer");
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedAttachmentExtensions.Contains(extension))
            {
                return ServiceResult<int>.BadRequest(
                    $"File type not allowed. Allowed types: {string.Join(", ", AllowedAttachmentExtensions)}");
            }

            bool systemExists = await _db.ComputerSystems.AnyAsync(c => c.Id == id);
            if (!systemExists)
            {
                return ServiceResult<int>.NotFound("That system does not exist");
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            Attachment attachment = new Attachment
            {
                OriginalName = fileName,
                ContentSize = file.Length,
                ContentType = file.ContentType,
                Content = ms.ToArray(),
                ComputerId = id
            };
            _db.Attachments.Add(attachment);
            await _db.SaveChangesAsync();

            return ServiceResult<int>.Ok(attachment.Id);
        }

        public async Task<ServiceResult<List<AttachmentDTO>>> ListAttachments(int id)
        {
            if (!await _db.ComputerSystems.AnyAsync(c => c.Id == id))
            {
                return ServiceResult<List<AttachmentDTO>>.NotFound("That system does not exist");
            }

            var attachments = await _db.Attachments
                .AsNoTracking()
                .Where(a => a.ComputerId == id)
                .OrderBy(a => a.UploadedAt)
                .ThenBy(a => a.Id)
                .Select(a => new AttachmentDTO
                {
                    Id = a.Id,
                    OriginalName = a.OriginalName,
                    ContentType = a.ContentType,
                    ContentSize = a.ContentSize,
                    UploadedAt = a.UploadedAt
                }).ToListAsync();

            return ServiceResult<List<AttachmentDTO>>.Ok(attachments);
        }

        public async Task<ServiceResult<AttachmentFileDTO>> DownloadAttachment(int id, int attachmentId)
        {
            var file = await _db.Attachments
                .AsNoTracking()
                .Where(a => a.Id == attachmentId && a.ComputerId == id)
                .Select(a => new AttachmentFileDTO
                {
                    OriginalName = a.OriginalName,
                    ContentType = a.ContentType,
                    Content = a.Content
                }).FirstOrDefaultAsync();

            if (file == null)
            {
                return ServiceResult<AttachmentFileDTO>.NotFound("That attachment does not exist");
            }

            return ServiceResult<AttachmentFileDTO>.Ok(file);
        }

        public async Task<ServiceResult<bool>> DeleteAttachment(int id, int attachmentId)
        {
            Attachment? attachment = await _db.Attachments
                .Where(a => a.Id == attachmentId && a.ComputerId == id)
                .FirstOrDefaultAsync();

            if (attachment == null)
            {
                return ServiceResult<bool>.NotFound("That attachment does not exist");
            }

            _db.Attachments.Remove(attachment);
            await _db.SaveChangesAsync();

            return ServiceResult<bool>.NoContent();
        }

        public async Task<ServiceResult<bool>> UpdateSystem(int id, UpdateSystemDTO dto)
        {
            ComputerSystem? system = await _db.ComputerSystems.FirstOrDefaultAsync(s => s.Id == id);

            if (system == null)
            {
                return ServiceResult<bool>.NotFound("That system does not exist");
            }

            if (dto.Name != null)
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ServiceResult<bool>.BadRequest("Name cannot be empty");
                }

                if (await _db.ComputerSystems.AnyAsync(s => s.Name == dto.Name && s.Id != id))
                {
                    return ServiceResult<bool>.Conflict($"A system named {dto.Name} already exists");
                }

                system.Name = dto.Name;
            }

            if (dto.DepartmentId != null)
            {
                if (!await _db.Departments.AnyAsync(d => d.Id == dto.DepartmentId))
                {
                    return ServiceResult<bool>.NotFound($"Department with id {dto.DepartmentId} does not exist");
                }

                system.DepartmentId = dto.DepartmentId.Value;
            }

            if (dto.OwnerId != null)
            {
                Employee? owner = await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.OwnerId);

                if (owner == null)
                {
                    return ServiceResult<bool>.NotFound($"Owner/Employee with id {dto.OwnerId} does not exist");
                }

                if (!owner.IsActive && owner.Id != system.OwnerId)
                {
                    return ServiceResult<bool>.BadRequest($"Owner/Employee with id {dto.OwnerId} is inactive");
                }

                system.OwnerId = dto.OwnerId.Value;
            }

            await _db.SaveChangesAsync();
            return ServiceResult<bool>.NoContent();
        }
    }
}
