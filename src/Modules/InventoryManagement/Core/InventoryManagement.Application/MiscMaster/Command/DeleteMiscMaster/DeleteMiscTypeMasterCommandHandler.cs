using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.MiscMaster.Command.DeleteMiscMaster
{
    public class DeleteMiscTypeMasterCommandHandler  : IRequestHandler<DeleteMiscMasterCommand, bool>
    {

         private readonly IMiscMasterCommandRepository _miscMasterCommandRepository;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;



        public DeleteMiscTypeMasterCommandHandler(IMiscMasterCommandRepository miscMasterCommandRepository, IMapper imapper, IMediator mediator)
        {
            _miscMasterCommandRepository = miscMasterCommandRepository;
            _imapper = imapper;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMiscMasterCommand request, CancellationToken cancellationToken)
        {
            // Map the request to the entity
            var miscMaster = _imapper.Map<InventoryManagement.Domain.Entities.MiscMaster>(request);

            // Perform the delete operation
            var isDeleted = await _miscMasterCommandRepository.DeleteAsync(request.Id, miscMaster);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: miscMaster.Id.ToString(),
                actionName: miscMaster.IsDeleted.ToString(),
                details: $"MiscMaster with ID {miscMaster.Id} was deleted.",
                module: "MiscMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            
             return isDeleted ? true : throw new ExceptionRules("MiscMaster deletion failed.");
                
            
        }
        
        
    }
}