using System.Text.Json;
using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.ILogService;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseIndents
{
    public class PurchaseIndentCommandRepository : IPurchaseIndentCommand
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogServiceCommand _logServiceCommand;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMapper _imapper;
        private readonly ILogger<PurchaseIndentCommandRepository> _logger;
        public PurchaseIndentCommandRepository(ApplicationDbContext dbContext, ILogServiceCommand logServiceCommand,
        IMiscMasterQueryRepository miscMasterQueryRepository, IMapper imapper, ILogger<PurchaseIndentCommandRepository> logger)
        {
            _dbContext = dbContext;
            _logServiceCommand = logServiceCommand;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _imapper = imapper;
            _logger = logger;
        }
        public async Task<IndentHeader> CreateAsync(IndentHeader indentHeader)
        {
            _dbContext.Entry(indentHeader);
            await _dbContext.IndentHeader.AddAsync(indentHeader);
            await _dbContext.SaveChangesAsync();

            return indentHeader;
        }

        public async Task<bool> DeleteAsync(int id, IndentHeader indentHeader)
        {
            var PurchaseIndentDelete = await _dbContext.IndentHeader.FirstOrDefaultAsync(u => u.Id == id);
            if (PurchaseIndentDelete != null)
            {
                PurchaseIndentDelete.IsDeleted = indentHeader.IsDeleted;
                return await _dbContext.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> UpdateAsync(IndentHeader indentHeader, string request)
        {
            

            var existingPurchaseIndent = await _dbContext.IndentHeader
             .Include(cf => cf.IndentDetails)
           .FirstOrDefaultAsync(u => u.Id == indentHeader.Id);

            var Indent = _imapper.Map<UpdatePurchaseIndentCommand>(existingPurchaseIndent);

            var StatusMisc = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Open);
            _logger.LogInformation("Update Purchase Indent. Old data: {@Indent}, New data: {@request}", Indent, request);
            // var IndentLog = new IndentLog
            // {
            //     IndentHeaderId = indentHeader.Id,
            //     ActionType = "Updated",
            //     ActionRemarks = "Indent Updated",
            //     PreviousData = JsonSerializer.Serialize(Indent),
            //     NewData = request,
            //     StatusId = StatusMisc.Id
            // };

            // await _logServiceCommand.CreateAsync(IndentLog);

            if (existingPurchaseIndent != null)
            {


                existingPurchaseIndent.IndentTypeId = indentHeader.IndentTypeId;
                existingPurchaseIndent.IndentDate = indentHeader.IndentDate;
                existingPurchaseIndent.UnitId = indentHeader.UnitId;
                existingPurchaseIndent.DepartmentId = indentHeader.DepartmentId;
                existingPurchaseIndent.Purpose = indentHeader.Purpose;
                existingPurchaseIndent.StatusId = indentHeader.StatusId;

                   
               // DELETE Removed Child Rows
                   
                var existingDetailIds = existingPurchaseIndent.IndentDetails.Select(x => x.Id).ToList();
                var updatedDetailIds = indentHeader.IndentDetails.Select(x => x.Id).ToList();

                var deletedIds = existingDetailIds.Except(updatedDetailIds).ToList();

                if (deletedIds.Any())
                {
                    var deletedDetails = existingPurchaseIndent.IndentDetails
                        .Where(x => deletedIds.Contains(x.Id))
                        .ToList();

                    _dbContext.IndentDetail.RemoveRange(deletedDetails);
                }

                foreach (var updatedDetail in indentHeader.IndentDetails)
                {
                    var existingDetail = existingPurchaseIndent.IndentDetails
                        .FirstOrDefault(d => d.Id == updatedDetail.Id);

                    if (existingDetail != null)
                    {
                        existingDetail.ItemId = updatedDetail.ItemId;
                        existingDetail.ItemCategoryId = updatedDetail.ItemCategoryId;
                        existingDetail.ItemUOMId = updatedDetail.ItemUOMId;
                        existingDetail.Rate = updatedDetail.Rate;
                        existingDetail.QuantityRequired = updatedDetail.QuantityRequired;
                        existingDetail.RequiredDate = updatedDetail.RequiredDate;
                        existingDetail.TotalEstimatedCost = updatedDetail.TotalEstimatedCost;
                        existingDetail.PRConsumptionDays = updatedDetail.PRConsumptionDays;
                        existingDetail.Remark = updatedDetail.Remark;
                        existingDetail.StatusId = updatedDetail.StatusId;

                    }
                    else
                    {

                        existingPurchaseIndent.IndentDetails.Add(updatedDetail);
                    }
                }


                return await _dbContext.SaveChangesAsync() > 0;
            }

            return true;
        }
        // public async Task<List<IndentDetail>> UpdateIndentDetailAsync(List<IndentDetail> indentDetail)
        // {
        //     if (indentDetail == null || indentDetail.Count == 0)
        //         return new List<IndentDetail>();

        //     var ids = indentDetail.Select(d => d.Id).ToList();

        //     var existing = await _dbContext.IndentDetail
        //         .Where(d => ids.Contains(d.Id))
        //         .ToListAsync();


        //     foreach (var entity in existing)
        //     {
        //         var incoming = indentDetail.FirstOrDefault(u => u.Id == entity.Id);
        //         if (incoming == null) continue;

        //         entity.ApprovedQuantity = incoming.ApprovedQuantity;

        //     }

        //     await _dbContext.SaveChangesAsync();
        //     return existing;

        // }
         public async Task<bool> RollbackStatusAsync(int id)
        {
            var existingPurchaseIndent = await _dbContext.IndentHeader
             .Include(cf => cf.IndentDetails)
           .FirstOrDefaultAsync(u => u.Id == id);

            var Indent = _imapper.Map<UpdatePurchaseIndentCommand>(existingPurchaseIndent);

            var StatusMisc = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Draft);
            var IndentLog = new IndentLog
            {
                IndentHeaderId = id,
                ActionType = "RollBack",
                ActionRemarks = "Indent Updated",
                PreviousData = JsonSerializer.Serialize(Indent),
                NewData = MiscEnumEntity.Draft,
                StatusId = StatusMisc.Id
            };

            await _logServiceCommand.CreateAsync(IndentLog);

            if (existingPurchaseIndent != null)
            {


                existingPurchaseIndent.StatusId = StatusMisc.Id;

                foreach (var updatedDetail in existingPurchaseIndent.IndentDetails)
                {
                    var existingDetail = existingPurchaseIndent.IndentDetails
                        .FirstOrDefault(d => d.Id == updatedDetail.Id);

                    if (existingDetail != null)
                    {
                        existingDetail.StatusId = StatusMisc.Id;

                    }
                    else
                    {

                        existingPurchaseIndent.IndentDetails.Add(updatedDetail);
                    }
                }


                return await _dbContext.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<bool> FinalizeStatus(IndentHeader indentHeader)
        {
            var existingPurchaseIndent = await _dbContext.IndentHeader
                .Include(h => h.IndentDetails)
                .FirstOrDefaultAsync(h => h.Id == indentHeader.Id);

            if (existingPurchaseIndent == null)
                return false;

            // Update header status
            existingPurchaseIndent.StatusId = indentHeader.StatusId;

            var incomingDetails = indentHeader.IndentDetails ?? new List<IndentDetail>();

            // ✅ CASE 1: no specific line list -> apply header status to ALL lines
            if (incomingDetails.Count == 0)
            {
                foreach (var d in existingPurchaseIndent.IndentDetails)
                    d.StatusId = indentHeader.StatusId;

                return await _dbContext.SaveChangesAsync() > 0;
            }

            // ✅ CASE 2: line-level updates provided
            foreach (var updatedDetail in incomingDetails)
            {
                var existingDetail = existingPurchaseIndent.IndentDetails
                    .FirstOrDefault(d => d.Id == updatedDetail.Id);

                if (existingDetail != null)
                    existingDetail.StatusId = updatedDetail.StatusId;
            }

            return await _dbContext.SaveChangesAsync() > 0;
        }

            public async Task<bool> UpdateRFQStatusAsync(IEnumerable<int> indentDetailIds)
            {
                if (indentDetailIds == null || !indentDetailIds.Any())
                    return false;

                var indentDetails = await _dbContext.IndentDetail
                    .Where(d => indentDetailIds.Contains(d.Id))
                    .ToListAsync();

                if (!indentDetails.Any())
                    return false;

                foreach (var detail in indentDetails)
                {
                    detail.IsRFQDone = true; 
                }

                await _dbContext.SaveChangesAsync();
                return true;
            }
    }
}