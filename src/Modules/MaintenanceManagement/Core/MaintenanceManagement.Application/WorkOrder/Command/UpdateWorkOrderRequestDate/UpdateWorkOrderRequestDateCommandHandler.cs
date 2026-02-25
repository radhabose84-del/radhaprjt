
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MediatR;

namespace MaintenanceManagement.Application.Maintenance.WorkOrder.Command.UpdateWorkOrderRequestDate
{
    public class UpdateWorkOrderRequestDateCommandHandler 
        : IRequestHandler<UpdateWorkOrderRequestDateCommand, bool>
    {
        private readonly IWorkOrderCommandRepository _workOrderCommandRepository;

        public UpdateWorkOrderRequestDateCommandHandler(
            IWorkOrderCommandRepository workOrderCommandRepository)
        {
            _workOrderCommandRepository = workOrderCommandRepository;
        }

        public async Task<bool> Handle(
            UpdateWorkOrderRequestDateCommand request,
            CancellationToken cancellationToken)
        {
            
            return await _workOrderCommandRepository.UpdateRequestDateAsync(
                request.WorkOrderId,
                request.RequestDate,
                request.IsSystemTime,
                cancellationToken);
        }
    }
}
