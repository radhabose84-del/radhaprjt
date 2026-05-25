#nullable disable

using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder.CreateSchedule;
using FluentValidation;

namespace MaintenanceManagement.Presentation.Validation.WorkOrder
{
    public class CreateWOScheduleCommandValidator : AbstractValidator<CreateWOScheduleCommand>
    {
        private readonly IWorkOrderCommandRepository _workOrderRepository;
        private readonly IWorkOrderQueryRepository _workOrderQueryRepository;

        public CreateWOScheduleCommandValidator(IWorkOrderCommandRepository workOrderRepository, IWorkOrderQueryRepository workOrderQueryRepository)
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

                RuleFor(x => x.WOSchedule.StartTime)
                    .NotEmpty()
                    .WithMessage("StartTime is required.");

                /*   RuleFor(x => x.WOSchedule)
                    .Must(schedule =>
                        schedule.EndTime.HasValue && schedule.StartTime < schedule.EndTime)
                    .WithMessage("StartTime must be before EndTime.")
                    .When(schedule => schedule.WOSchedule.EndTime.HasValue); */
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

        private async Task<bool> IsClosingTransitionAsync(CreateWOScheduleCommand cmd, CancellationToken ct)
        {
            if (cmd.WOSchedule == null) return false;
            var closedId = await GetClosedStatusIdAsync();
            return closedId.HasValue && cmd.WOSchedule.StatusId == closedId.Value;
        }

        private async Task<int?> GetClosedStatusIdAsync()
        {
            var statuses = await _workOrderQueryRepository.GetWOStatusDescAsync();
            return statuses?.FirstOrDefault(s => string.Equals(s.Code, "Closed", System.StringComparison.OrdinalIgnoreCase))?.Id;
        }
    }
}