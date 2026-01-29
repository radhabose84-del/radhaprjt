using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.Exceptions;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.CostCenter.Command.UpdateCostCenter
{
    public class UpdateCostCenterCommandHandler  : IRequestHandler<UpdateCostCenterCommand, int>
    {
        private readonly ICostCenterCommandRepository _iCostCenterCommandRepository;
        private readonly ICostCenterQueryRepository _iCostCenterQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _mediator; 

        public UpdateCostCenterCommandHandler(ICostCenterCommandRepository iCostCenterCommandRepository, ICostCenterQueryRepository iCostCenterQueryRepository, IMapper imapper, IMediator mediator)
        {
            _iCostCenterCommandRepository = iCostCenterCommandRepository;
            _iCostCenterQueryRepository = iCostCenterQueryRepository;
            _Imapper = imapper;
            _mediator = mediator;
        }

        public async Task<int> Handle(UpdateCostCenterCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0) // Inactive
            {
                var linked = await _iCostCenterQueryRepository.IsCostCenterLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }

       
            var costCenter = _Imapper.Map<MaintenanceManagement.Domain.Entities.CostCenter>(request);
            var result = await _iCostCenterCommandRepository.UpdateAsync(request.Id, costCenter);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: costCenter.CostCenterCode,
                actionName: costCenter.CostCenterName,
                details: $"CostCenter details was updated",
                module: "CostCenter");
            await _mediator.Publish(domainEvent, cancellationToken);
           
            return result > 0 ? result : throw new ExceptionRules("CostCenter update failed.");   
        }
    }
}