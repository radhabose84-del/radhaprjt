using AutoMapper;
using BudgetManagement.Application.BudgetGroups.Commands.CreateBudgetGroup;
using Contracts.Common;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Events;
using MediatR;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;

namespace BudgetManagement.Application.BudgetGroups.Command.CreateBudgetGroup
{
    public class CreateBudgetGroupCommandHandler : IRequestHandler<CreateBudgetGroupCommand, int>
    {
        private readonly IBudgetGroupCommandRepository _budgetGroupCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepo;

        public CreateBudgetGroupCommandHandler(
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

        public async Task<int> Handle(CreateBudgetGroupCommand request, CancellationToken cancellationToken)
        {
            var exists = await _budgetGroupCommandRepository.ExistsByNameAndUnitDepartmentAsync(
                request.Name.Trim(),
                request.UnitId,
                request.DepartmentId,
                cancellationToken);

            if (exists)
            {
                throw new ExceptionRules("Budget Group name already exists for this Unit and Department.");
            }

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetGroup>(request);

            // Fetch the AllocationRuleIds dynamically from MiscMaster using MiscEnumEntity
            var allocationRuleByPercentage = await _miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypePercentage);
            var allocationRuleBySpindle = await _miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeSpindle);
            var allocationRuleByRequest = await _miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeRequest);
            var annual = await _miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeAnnual);
            var monthly = await _miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeMonthly);

            if (allocationRuleBySpindle != null && request.AllocatedSpindleCost.HasValue && request.AllocatedSpindleCost.Value > 0)
            {
                // Use Spindle AllocationRuleId dynamically
                entity.AllocationRuleId = allocationRuleBySpindle.Id;
            }
            else if (allocationRuleByPercentage != null && request.AllocatedPercentage.HasValue && request.AllocatedPercentage.Value > 0)
            {
                // Use Percentage AllocationRuleId dynamically
                entity.AllocationRuleId = allocationRuleByPercentage.Id;
            }
            else if (allocationRuleByRequest != null && !request.AllocatedPercentage.HasValue && !request.AllocatedSpindleCost.HasValue)
            {
                // Use Request AllocationRuleId dynamically if neither percentage nor spindle cost is provided
                entity.AllocationRuleId = allocationRuleByRequest.Id;
            }
            else
            {
                // No valid allocation rule found or both values are missing, throw validation error
                throw new ExceptionRules("Either AllocatedPercentage or AllocatedSpindleCost must be provided, or select a valid allocation type.");
            }

            if ((annual == null || request.BudgetTypeId != annual.Id) &&
            (monthly == null || request.BudgetTypeId != monthly.Id))
            {
                throw new ExceptionRules("Invalid Budget Type (Annual/Monthly).");
            }
            entity.BudgetTypeId = request.BudgetTypeId;
            entity.CarryForward = request.CarryForward;


            // Handle IsParent logic
            // Handle IsParent logic only when needed, but always pass ParentBudgetGroupId correctly
            if (request.IsParent && !request.ParentBudgetGroupId.HasValue)
            {
                // If IsParent is true and no parent is selected, we allow ParentBudgetGroupId to be null
                entity.ParentBudgetGroupId = null;
            }
            else if (!request.IsParent && request.ParentBudgetGroupId.HasValue)
            {
                // If IsParent is false and a parent is selected, correctly pass the ParentBudgetGroupId
                entity.ParentBudgetGroupId = request.ParentBudgetGroupId;
            }
            else if (request.IsParent && request.ParentBudgetGroupId.HasValue)
            {
                // If IsParent is true but parent is selected, we assign the correct ParentBudgetGroupId
                entity.ParentBudgetGroupId = request.ParentBudgetGroupId;
            }

            entity.IsDeleted = BaseEntity.IsDelete.NotDeleted;

            var resultId = await _budgetGroupCommandRepository.CreateAsync(entity, cancellationToken);

            if (resultId <= 0)
            {
                throw new ExceptionRules("Budget Group creation failed.");
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "CreateBudgetGroup",
                actionCode: resultId.ToString(),
                actionName: entity.Name ?? string.Empty,
                details: "Budget Group details were created.",
                module: "Budget Group");

            await _mediator.Publish(domainEvent, cancellationToken);

            return resultId;
        }


    }
}
