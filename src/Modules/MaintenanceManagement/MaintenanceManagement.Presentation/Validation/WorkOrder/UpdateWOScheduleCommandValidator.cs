#nullable disable


using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder.UpdateSchedule;
using FluentValidation;

namespace MaintenanceManagement.Presentation.Validation.WorkOrder
{
    public class UpdateWOScheduleCommandValidator  : AbstractValidator<UpdateWOScheduleCommand>
    {
        private readonly IWorkOrderCommandRepository _workOrderRepository;
        private readonly IWorkOrderQueryRepository _workOrderQueryRepository;

        public UpdateWOScheduleCommandValidator(IWorkOrderCommandRepository workOrderRepository, IWorkOrderQueryRepository workOrderQueryRepository)
        {
            _workOrderRepository = workOrderRepository;
            _workOrderQueryRepository = workOrderQueryRepository;

            RuleFor(x => x.WOSchedule)
                .NotNull()
                .WithMessage("WorkOrder schedule cannot be null.");

            When(x => x.WOSchedule != null, () =>
            {
                RuleFor(x => x.WOSchedule.WorkOrderId)
                    .NotNull()
                    .WithMessage("WorkOrderId is required.")
                    .MustAsync(WorkOrderExists)
                    .WithMessage("WorkOrderId does not exist.");

                RuleFor(x => x.WOSchedule.EndTime)
                    .NotNull()
                    .WithMessage("EndTime is required.");

            });

            WhenAsync(IsClosingTransitionAsync, () =>
            {
                RuleFor(x => x.WOSchedule.StartTime)
                    .NotEqual(default(DateTimeOffset))
                    .WithMessage("Maintenance Start Time is required when closing the Work Order.");

                RuleFor(x => x.WOSchedule.EndTime)
                    .NotNull()
                    .WithMessage("Maintenance End Time is required when closing the Work Order.");

                RuleFor(x => x.WOSchedule)
                    .Must(sch => sch.EndTime == null
                              || sch.StartTime == default(DateTimeOffset)
                              || sch.StartTime < sch.EndTime.Value)
                    .WithMessage("Maintenance Start Time must be earlier than Maintenance End Time.");
            });
        }

        private async Task<bool> WorkOrderExists(int? workOrderId, CancellationToken cancellationToken)
        {
            if (!workOrderId.HasValue)
                return false;

            var workOrder = await _workOrderRepository.GetByIdAsync(workOrderId.Value);
            return workOrder != null;
        }

        private async Task<bool> IsClosingTransitionAsync(UpdateWOScheduleCommand cmd, CancellationToken ct)
        {
            if (cmd.WOSchedule == null) return false;

            var closedId = await GetClosedStatusIdAsync();
            if (!closedId.HasValue || cmd.WOSchedule.StatusId != closedId.Value)
                return false;

            // Time-tracking validation applies only to Preventive Schedule WOs.
            // Request-driven WOs (Breakdown / Predictive / Internal Request) have
            // no Maintenance Start/End Time fields in the UI and must skip this rule.
            if (!cmd.WOSchedule.WorkOrderId.HasValue) return false;
            var wo = await _workOrderRepository.GetByIdAsync(cmd.WOSchedule.WorkOrderId.Value);
            return wo != null && wo.PreventiveScheduleId.HasValue;
        }

        private async Task<int?> GetClosedStatusIdAsync()
        {
            var statuses = await _workOrderQueryRepository.GetWOStatusDescAsync();
            return statuses?.FirstOrDefault(s => string.Equals(s.Code, "Closed", System.StringComparison.OrdinalIgnoreCase))?.Id;
        }
    }
}