using MediatR;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using BudgetManagement.Application.BudgetAllocation.Command.Update;

namespace BudgetManagement.Application.BudgetAllocation.Command.UpsertOnApproval
{
    public sealed class UpsertBudgetAllocationOnApprovalCommandHandler
        : IRequestHandler<UpsertBudgetAllocationOnApprovalCommand, bool>
    {
        private readonly IBudgetRequestCommandRepository _budgetRequestRepo;
        private readonly IBudgetAllocationQueryRepository _allocationQuery;
        private readonly IBudgetAllocationCommandRepository _allocationCmd;
        private readonly IMiscMasterQueryRepository _misc;

        public UpsertBudgetAllocationOnApprovalCommandHandler(
            IBudgetRequestCommandRepository budgetRequestRepo,
            IBudgetAllocationQueryRepository allocationQuery,
            IBudgetAllocationCommandRepository allocationCmd,
            IMiscMasterQueryRepository misc)
        {
            _budgetRequestRepo = budgetRequestRepo;
            _allocationQuery = allocationQuery;
            _allocationCmd = allocationCmd;
            _misc = misc;
        }

        public async Task<bool> Handle(UpsertBudgetAllocationOnApprovalCommand request, CancellationToken ct)
        {
            var br = await _budgetRequestRepo.GetByIdAsync(request.BudgetRequestId, ct);
            if (br == null) return false;

            //if (br.BudgetGroupId is null || br.BudgetGroupId.Value <= 0) return false;
            if (br.RequestById is null || br.RequestById.Value <= 0) return false;

            // AllocationTypeId -> MiscMaster (AllocationType, "Request")
            var allocationType = await _misc.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeRequest);
            var allocationTypeId = allocationType?.Id ?? 0;

            var existing = await _allocationCmd.GetByKeyAsync(
                unitId: br.UnitId,
                financialYearId: br.FinancialYearId,
                requestMonthId: br.RequestMonthId,
                budgetGroupId: br.BudgetGroupId,
                requestById: br.RequestById.Value,      
                fromDate: br.FromDate??DateOnly.MinValue,
                toDate: br.ToDate??DateOnly.MinValue,   
                wbsId: br.WBSId,
                projectId: br.ProjectId,
                ct);

            // ✅ If exists -> update ONLY RemainingBalance
            if (existing != null)
            {
                var newBalance = (existing.RemainingBalance ?? 0m) + br.RequestAmount;
                return await _allocationCmd.UpdateRemainingBalanceAsync(existing.Id, newBalance, ct);
            }

            // ✅ Else create new allocation row
            var entity = new BudgetManagement.Domain.Entities.BudgetAllocation
            {
                UnitId = br.UnitId,
                FinancialYearId = br.FinancialYearId,
                RequestMonthId = br.RequestMonthId,
                RequestById = br.RequestById.Value,
                RequestId = br.Id,

                BudgetGroupId = br.BudgetGroupId,
                AllocationTypeId = allocationTypeId,

                SpindleCount = 0,
                RatePerSpindle = 0,

                ApprovedAmount = br.RequestAmount,
                RemainingBalance = br.RequestAmount,
                Remarks = br.Remarks,
                FromDate = br.FromDate,
                ToDate = br.ToDate,
                CreatedBy = br.RequestById.Value,
                CreatedDate = DateTimeOffset.UtcNow,
                ProjectId = br.ProjectId,
                WBSId = br.WBSId
            };

            await _allocationCmd.CreateAsync(entity);
            return true;
        }
    }
}
