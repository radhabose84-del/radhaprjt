using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Commands.DeleteService
{
    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, bool>
    {
       private readonly IServiceQueryRepository _queryRepo;
        private readonly IServiceCommandRepository _commandRepo;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public DeleteServiceCommandHandler(IServiceQueryRepository queryRepo, IServiceCommandRepository commandRepo, IMediator mediator, IMapper mapper)
        {
            _queryRepo = queryRepo;
            _commandRepo = commandRepo;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<bool> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
           // Map the request to the entity
            var serviceMaster = _mapper.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(request);

            // Perform the delete operation
            var isDeleted = await _commandRepo.SoftDeleteAsync(serviceMaster, cancellationToken);

            // Domain Event,
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: serviceMaster.Id.ToString(),
                actionName: serviceMaster.IsDeleted.ToString(),
                details: $"ServiceMaster with ID {serviceMaster.Id} was deleted.",
                module: "ServiceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            
             return isDeleted ? true : throw new ExceptionRules("ServiceMaster deletion failed.");
        }
    }
}