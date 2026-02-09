using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IProjectMaster;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Infrastructure.Data;
using System.Linq;
using System.Threading;
using Core.Domain.Common;
using Core.Application.Common.Interfaces.IMiscTypeMaster;
using AutoMapper;
using Core.Application.ProjectMaster.Command.UpdateProjectMaster;
using Core.Application.Common.Interfaces.IMiscMaster;

namespace ProjectManagement.Infrastructure.Repositories.ProjectMaster
{
    public class ProjectMasterCommandRepository : IProjectMasterCommandRepository
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        private readonly IMapper _mapper;

        public ProjectMasterCommandRepository(ApplicationDbContext dbContext, IMiscMasterQueryRepository misc, IMapper mapper)
        {
            _dbContext = dbContext;
            _miscMasterQueryRepository = misc;
            _mapper = mapper;
        }
        public async Task<Core.Domain.Entities.ProjectMaster> CreateAsync(Core.Domain.Entities.ProjectMaster projectMaster, CancellationToken ct = default)
        {
            if (projectMaster.CreatedDate == default)
                projectMaster.CreatedDate = DateTimeOffset.UtcNow;


            // 🔹 Generate ProjectCode BEFORE saving (never null)
            projectMaster.ProjectCode = await GenerateProjectCodeAsync(projectMaster, ct);

            var pending = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
            projectMaster.StatusId = pending.Id;

            await _dbContext.ProjectMaster.AddAsync(projectMaster, ct);
            await _dbContext.SaveChangesAsync(ct);

            return projectMaster;
        }


        private async Task<string> GenerateProjectCodeAsync(Core.Domain.Entities.ProjectMaster project, CancellationToken ct)
        {
            // Decide what date to base the code on (explicitly DateTimeOffset)


            DateTimeOffset basisDate = DateTimeOffset.UtcNow;
            var yearMonth = $"{basisDate:yyyyMM}";

            // 🔹 Prefix pattern – customize as per your business needs
            var prefix = $"PROJ-{yearMonth}-";

            // 🔹 Find the last used code with this prefix
            var lastCode = await _dbContext.ProjectMaster
                .AsNoTracking()
                .Where(p => p.ProjectCode.StartsWith(prefix))
                .OrderByDescending(p => p.ProjectCode)
                .Select(p => p.ProjectCode)
                .FirstOrDefaultAsync(ct);

            var nextSeq = 1;

            if (!string.IsNullOrWhiteSpace(lastCode))
            {
                // Expecting format: PROJ-202512-0007
                var parts = lastCode.Split('-');
                var lastSeqPart = parts.Length == 3 ? parts[2] : "0";

                if (int.TryParse(lastSeqPart, out var lastSeq))
                    nextSeq = lastSeq + 1;
            }

            // 🔹 Final code: PROJ-202512-0001
            var newCode = $"{prefix}{nextSeq:D4}";

            return newCode;
        }

        public async Task UpdateAsync(Core.Domain.Entities.ProjectMaster projectMaster, CancellationToken ct = default)
        {
            _dbContext.ProjectMaster.Update(projectMaster);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<Core.Domain.Entities.ProjectMaster?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _dbContext.ProjectMaster
                .Include(p => p.ProjectDocuments) // Include related ProjectDocuments
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            return entity;

        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _dbContext.ProjectMaster
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (entity == null)
                return false;

            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            entity.IsActive = BaseEntity.Status.Inactive;

            _dbContext.ProjectMaster.Update(entity);
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> RollbackProjectStatusAsync(int id, CancellationToken ct = default)
        {
            var existingProject = await _dbContext.ProjectMaster
                .FirstOrDefaultAsync(p => p.Id == id
                                    && p.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

            if (existingProject == null)
                return false;

            var statusMisc = await _miscMasterQueryRepository
                .GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Draft);

            existingProject.StatusId = statusMisc.Id;

            _dbContext.ProjectMaster.Update(existingProject);

            return await _dbContext.SaveChangesAsync(ct) > 0;
        }

   
         public async Task<bool> UpdateProjectApprovalStatusAsync(int projectId, int statusId, CancellationToken ct = default)
        {
            var entity = await _dbContext.Set<Core.Domain.Entities.ProjectMaster>()
                .FirstOrDefaultAsync(x => x.Id == projectId && x.IsDeleted == 0, ct);

            if (entity == null) return false;

            entity.StatusId = statusId;
            entity.ModifiedDate = DateTimeOffset.Now;

            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}