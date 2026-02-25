using AutoMapper;
using BudgetManagement.Application.BudgetGroups.Commands.UpdateBudgetGroup;
using Contracts.Common;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Events;
using MediatR;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;

namespace BudgetManagement.Application.BudgetGroups.Command.UpdateBudgetGroup
{
    public class UpdateBudgetGroupCommandHandler : IRequestHandler<UpdateBudgetGroupCommand, int>
    {
        private readonly IBudgetGroupCommandRepository _budgetGroupCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepo;

        public UpdateBudgetGroupCommandHandler(
            IBudgetGroupCommandRepository budgetGroupCommandRepository,
            IMediator mediator,
            IMapper mapper,
            IMiscMasterQueryRepository miscMasterQueryRepo)
        {
            _budgetGroupCommandRepository = budgetGroupCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
            _miscMasterQueryRepo = miscMasterQueryRepo;
        }

        public async Task<int> Handle(UpdateBudgetGroupCommand request, CancellationToken cancellationToken)
        {
            // Fetch AllocationRule IDs dynamically from MiscMaster
            var allocationRuleByPercentageId = (await _miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypePercentage))?.Id;
            var allocationRuleBySpindleId = (await _miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeSpindle))?.Id;
            var allocationRuleByRequestId = (await _miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeRequest))?.Id;
            var annual  = await _miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeAnnual);
            var monthly = await _miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeMonthly);

            // 2) Load existing entity so audit fields (CreatedBy, CreatedByName, etc.) are preserved
            var entity = await _budgetGroupCommandRepository.GetByIdAsync(request.Id, cancellationToken);

            if (entity == null || entity.IsDeleted == BaseEntity.IsDelete.Deleted)
            {
                throw new ExceptionRules("Budget Group not found.");
            }

            if ((annual == null || request.BudgetTypeId != annual.Id) && (monthly == null || request.BudgetTypeId != monthly.Id))
            {
                throw new ExceptionRules("Invalid Budget Type (Annual/Monthly).");
            }

            entity.BudgetTypeId = request.BudgetTypeId;
            entity.CarryForward = request.CarryForward;

            // 3) Proper duplicate-name check (excluding the current Id)
            var isDuplicate = await _budgetGroupCommandRepository.IsNameDuplicateAsync(
                request.Name.Trim(),
                request.Id,
                request.UnitId,
                request.DepartmentId,
                cancellationToken);

            if (isDuplicate)
            {
                throw new ExceptionRules("Budget Group name already exists for this Unit and Department.");
            }

            // 4) Map incoming values into the existing entity (does NOT wipe audit fields)
            _mapper.Map(request, entity);

            // Validate the AllocationRuleId logic based on the "AllocationType" and values like "AllocatedSpindleCost" or "AllocatedPercentage"
            if (request.AllocationRuleId == null || request.AllocationRuleId == 0)
            {
                if (request.AllocatedSpindleCost.HasValue && request.AllocatedSpindleCost.Value > 0 && allocationRuleBySpindleId.HasValue)
                {
                    entity.AllocationRuleId = allocationRuleBySpindleId;
                }
                else if (request.AllocatedPercentage.HasValue && request.AllocatedPercentage.Value > 0 && allocationRuleByPercentageId.HasValue)
                {
                    entity.AllocationRuleId = allocationRuleByPercentageId;
                }
                else if (allocationRuleByRequestId.HasValue)
                {
                    entity.AllocationRuleId = allocationRuleByRequestId; // For request allocation type
                }
            }

            // Handle IsParent and ParentBudgetGroupId logic as before
            if (request.IsParent && !request.ParentBudgetGroupId.HasValue)
            {
                entity.ParentBudgetGroupId = null; // If no parent selected, clear the parent
            }
            else if (!request.IsParent && request.ParentBudgetGroupId.HasValue)
            {
                // If IsParent = false and ParentBudgetGroupId is selected, assign it correctly
                entity.ParentBudgetGroupId = request.ParentBudgetGroupId;
            }
            else if (request.IsParent && request.ParentBudgetGroupId.HasValue)
            {
                // If IsParent = true but parent is selected, assign the correct ParentBudgetGroupId
                entity.ParentBudgetGroupId = request.ParentBudgetGroupId;
            }

            // Ensure it stays as not deleted
            entity.IsDeleted = BaseEntity.IsDelete.NotDeleted;

            var resultId = await _budgetGroupCommandRepository.UpdateAsync(request.Id, entity, cancellationToken);

            if (resultId <= 0)
            {
                throw new ExceptionRules("Budget Group update failed.");
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "UpdateBudgetGroup",
                actionCode: resultId.ToString(),
                actionName: entity.Name ?? string.Empty,
                details: "Budget Group details were updated.",
                module: "BudgetGroup");

            await _mediator.Publish(domainEvent, cancellationToken);

            return resultId;
        }
    }
}
