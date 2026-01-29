
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder.CreateSchedule;
using FluentValidation;

namespace MaintenanceManagement.API.Validation.WorkOrder
{
    public class CreateWOScheduleCommandValidator : AbstractValidator<CreateWOScheduleCommand>
    {
        private readonly IWorkOrderCommandRepository _workOrderRepository;

        public CreateWOScheduleCommandValidator(IWorkOrderCommandRepository workOrderRepository)
        {
            _workOrderRepository = workOrderRepository;

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
        }

        private async Task<bool> WorkOrderExists(int? workOrderId, CancellationToken cancellationToken)
        {
            if (!workOrderId.HasValue)
                return false;

            var workOrder = await _workOrderRepository.GetByIdAsync(workOrderId.Value);
            return workOrder != null;
        }
    }
}