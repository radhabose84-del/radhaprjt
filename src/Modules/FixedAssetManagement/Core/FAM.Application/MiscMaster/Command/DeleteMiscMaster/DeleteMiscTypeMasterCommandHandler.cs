using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IMiscMaster;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using FAM.Domain.Events;
using MediatR;
using FluentValidation;

namespace FAM.Application.MiscMaster.Command.DeleteMiscMaster
{
    public class DeleteMiscTypeMasterCommandHandler  : IRequestHandler<DeleteMiscMasterCommand, bool>
    {

         private readonly IMiscMasterCommandRepository _miscMasterCommandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepo;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;



        public DeleteMiscTypeMasterCommandHandler(IMiscMasterCommandRepository miscMasterCommandRepository, IMiscMasterQueryRepository miscMasterQueryRepo, IMapper imapper, IMediator mediator)
        {
            _miscMasterCommandRepository = miscMasterCommandRepository;
            _miscMasterQueryRepo = miscMasterQueryRepo;
            _imapper = imapper;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMiscMasterCommand request, CancellationToken cancellationToken)
        {
            // Map the request to the entity
            var miscMaster = _imapper.Map<FAM.Domain.Entities.MiscMaster>(request);

            var linked = await _miscMasterQueryRepo.IsMiscMasterLinkedAsync(request.Id);
            if (linked)
            {
                throw new ValidationException("This master is linked with other records. You cannot delete this record.");
            }

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

            // Return the response based on the result
            if (isDeleted)
            {
                return isDeleted;
            }
            throw new Exception("MiscMaster not deleted.");
         
        }
        
    }
}