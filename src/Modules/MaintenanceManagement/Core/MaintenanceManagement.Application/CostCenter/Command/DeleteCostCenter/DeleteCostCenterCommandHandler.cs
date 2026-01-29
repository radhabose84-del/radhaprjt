using AutoMapper;
using MaintenanceManagement.Application.Common.Exceptions;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.CostCenter.Command.DeleteCostCenter
{
    public class DeleteCostCenterCommandHandler : IRequestHandler<DeleteCostCenterCommand, int>
    {
        private readonly ICostCenterCommandRepository _iCostCenterCommandRepository;
        private readonly ICostCenterQueryRepository _iCostCenterQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _mediator; 

        public DeleteCostCenterCommandHandler(ICostCenterCommandRepository iCostCenterCommandRepository, ICostCenterQueryRepository iCostCenterQueryRepository, IMapper imapper, IMediator mediator)
        {
            _iCostCenterCommandRepository = iCostCenterCommandRepository;
            _iCostCenterQueryRepository = iCostCenterQueryRepository;
            _Imapper = imapper;
            _mediator = mediator;
        }

        public async Task<int> Handle(DeleteCostCenterCommand request, CancellationToken cancellationToken)
        {
            
            var costCenterGroup = _Imapper.Map<MaintenanceManagement.Domain.Entities.CostCenter>(request);
            var result = await _iCostCenterCommandRepository.DeleteAsync(request.Id,costCenterGroup);

           // linked validation for delete
            var linked = await _iCostCenterQueryRepository.IsCostCenterLinkedAsync(request.Id);
            if (linked)
            { 
              throw new ValidationException("This master is linked with other records. You cannot delete this record.");
            }       

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: costCenterGroup.CostCenterCode,
                actionName: costCenterGroup.CostCenterName,
                details: $"CostCenter details was deleted",
                module: "CostCenter");
            await _mediator.Publish(domainEvent);
          

            return result > 0 ? result : throw new ExceptionRules("CostCenter not found.");
        }


    }
}