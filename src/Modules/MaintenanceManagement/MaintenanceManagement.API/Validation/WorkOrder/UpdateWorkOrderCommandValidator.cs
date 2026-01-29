

using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;

namespace MaintenanceManagement.API.Validation.WorkOrder
{
    public class UpdateWorkOrderCommandValidator  : AbstractValidator<UpdateWorkOrderCommand>
    {
        private readonly IWorkOrderCommandRepository _workOrderRepository;

        public UpdateWorkOrderCommandValidator(MaxLengthProvider maxLengthProvider,IWorkOrderCommandRepository workOrderRepository)
        {
            _workOrderRepository = workOrderRepository;

            RuleFor(x => x.WorkOrder).NotNull().WithMessage("WorkOrder cannot be null.");
            RuleFor(x => x.WorkOrder.Id)
                .MustAsync(WorkOrderExists)
                .WithMessage("Invalid WorkOrderId. WorkOrder does not exist.");

            When(x => x.WorkOrder != null, () =>
            {
                ApplyIdExclusivityRule();
                ApplyNotEmptyRules();
                ApplyMaxLengthRules(maxLengthProvider);
                ApplyNumericOnlyRules();
            });
        }

        private void ApplyIdExclusivityRule()
        {
            RuleFor(x => x.WorkOrder)
                .Must(x =>
                    (x.RequestId.HasValue && !x.PreventiveScheduleId.HasValue) ||
                    (!x.RequestId.HasValue && x.PreventiveScheduleId.HasValue))
                .WithMessage("Either RequestId or PreventiveScheduleId must be provided, not both.");
        }

        private void ApplyNotEmptyRules()
        {
            RuleForEach(x => x.WorkOrder.WorkOrderItem)
                .ChildRules(woItem =>
                {
                    woItem.RuleFor(x => x.AvailableQty).NotEmpty().WithMessage("AvailableQty is required.");
                    woItem.RuleFor(x => x.UsedQty).NotEmpty().WithMessage("UsedQty is required.");
                });

            RuleForEach(x => x.WorkOrder.WorkOrderActivity)
                .ChildRules(woActivity =>
                {
                    woActivity.RuleFor(x => x.ActivityId).NotEmpty().WithMessage("ActivityId is required.");
                });

            RuleForEach(x => x.WorkOrder.WorkOrderTechnician)
                .ChildRules(woTech =>
                {
                    woTech.RuleFor(x => x.HoursSpent).GreaterThanOrEqualTo(0).WithMessage("HoursSpent is required.");
                    woTech.RuleFor(x => x.MinutesSpent).GreaterThanOrEqualTo(0).WithMessage("MinutesSpent is required.");
                });
        }

        private void ApplyMaxLengthRules(MaxLengthProvider maxLengthProvider)
        {
            int woRemarksMax = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>("Remarks") ?? 1000;
            int woItemMax = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderItem>("ItemName") ?? 250;
            int woTechMax = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderTechnician>("TechnicianName") ?? 100;
            int woActivityMax = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderActivity>("Description") ?? 100;
            int woCheckMax = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderCheckList>("Description") ?? 1000;

            RuleFor(x => x.WorkOrder.Remarks)
                .MaximumLength(woRemarksMax)
                .WithMessage($"Remarks exceeded max length of {woRemarksMax}");

            RuleForEach(x => x.WorkOrder.WorkOrderItem)
                .ChildRules(woItem =>
                {
                    woItem.RuleFor(x => x.ItemName)
                        .MaximumLength(woItemMax)
                        .WithMessage($"ItemName exceeded max length of {woItemMax}");
                });

            RuleForEach(x => x.WorkOrder.WorkOrderTechnician)
                .ChildRules(woTech =>
                {
                    woTech.RuleFor(x => x.TechnicianName)
                        .MaximumLength(woTechMax)
                        .WithMessage($"TechnicianName exceeded max length of {woTechMax}");
                });

            RuleForEach(x => x.WorkOrder.WorkOrderActivity)
                .ChildRules(woActivity =>
                {
                    woActivity.RuleFor(x => x.Description)
                        .MaximumLength(woActivityMax)
                        .WithMessage($"Activity Description exceeded max length of {woActivityMax}");
                });

            RuleForEach(x => x.WorkOrder.WorkOrderCheckList)
                .ChildRules(woCheck =>
                {
                    woCheck.RuleFor(x => x.Description)
                        .MaximumLength(woCheckMax)
                        .WithMessage($"CheckList Description exceeded max length of {woCheckMax}");
                });
        }

        private void ApplyNumericOnlyRules()
        {
            var numericRegex = new System.Text.RegularExpressions.Regex(@"^\d+$");

            RuleFor(x => x.WorkOrder.TotalManPower)
                .InclusiveBetween(1, int.MaxValue)
                .WithMessage("TotalManPower must be a positive integer.");

            RuleFor(x => x.WorkOrder.TotalSpentHours)
                .InclusiveBetween(1, int.MaxValue)
                .WithMessage("TotalSpentHours must be a positive number.");

            RuleForEach(x => x.WorkOrder.WorkOrderItem)
                .ChildRules(woItem =>
                {
                    woItem.RuleFor(x => x.AvailableQty.ToString())
                        .Matches(numericRegex)
                        .WithMessage("AvailableQty must be numeric.");
                    
                    woItem.RuleFor(x => x.UsedQty.ToString())
                        .Matches(numericRegex)
                        .WithMessage("UsedQty must be numeric.");
                });
        }
        private async Task<bool> WorkOrderExists(int workOrderId, CancellationToken cancellationToken)
        {
            var workOrder = await _workOrderRepository.GetByIdAsync(workOrderId);
            return workOrder != null;
        }
    }
}