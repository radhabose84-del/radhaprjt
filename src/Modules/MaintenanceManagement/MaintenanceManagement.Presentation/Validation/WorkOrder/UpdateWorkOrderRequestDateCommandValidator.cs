using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.Maintenance.WorkOrder.Command.UpdateWorkOrderRequestDate;
using FluentValidation;

public class UpdateWorkOrderRequestDateCommandValidator 
    : AbstractValidator<UpdateWorkOrderRequestDateCommand>
{
    private readonly IWorkOrderQueryRepository _workOrderQueryRepository;

    public UpdateWorkOrderRequestDateCommandValidator(
        IWorkOrderQueryRepository workOrderQueryRepository)
    {
        _workOrderQueryRepository = workOrderQueryRepository;

        RuleFor(x => x.WorkOrderId)
            .GreaterThan(0)
            .WithMessage("WorkOrderId is required.");

        RuleFor(x => x)
            .MustAsync(async (cmd, cancellation) =>
                await _workOrderQueryRepository.ValidateRequestDateAsync(
                    cmd.WorkOrderId,
                    cmd.RequestDate,
                    cmd.IsSystemTime,
                    cancellation))
            .WithMessage("Request date cannot be less than the schedule EndDate.");
    }
}
