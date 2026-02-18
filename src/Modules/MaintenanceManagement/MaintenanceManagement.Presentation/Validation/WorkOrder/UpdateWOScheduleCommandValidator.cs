#nullable disable


using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder.UpdateSchedule;
using FluentValidation;

namespace MaintenanceManagement.Presentation.Validation.WorkOrder
{
    public class UpdateWOScheduleCommandValidator  : AbstractValidator<UpdateWOScheduleCommand>
    {
        private readonly IWorkOrderCommandRepository _workOrderRepository;

        public UpdateWOScheduleCommandValidator(IWorkOrderCommandRepository workOrderRepository)
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

                RuleFor(x => x.WOSchedule.EndTime)
                    .NotNull()
                    .WithMessage("EndTime is required.");
             
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