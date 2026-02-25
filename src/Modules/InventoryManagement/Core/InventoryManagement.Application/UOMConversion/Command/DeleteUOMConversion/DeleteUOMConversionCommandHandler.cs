using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion
{
    public class DeleteUOMConversionCommandHandler : IRequestHandler<DeleteUOMConversionCommand, bool>
    {

        private readonly IUOMConversionCommandRepository _iUOMConversionCommandRepository;
        private readonly IUOMConversionQueryRepository _uOMConversionQueryRepository;

        private readonly IMapper _imapper;
        private readonly IMediator _mediator;

        public DeleteUOMConversionCommandHandler(IUOMConversionCommandRepository iUOMConversionCommandRepository, IUOMConversionQueryRepository uOMConversionQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iUOMConversionCommandRepository = iUOMConversionCommandRepository;
            _uOMConversionQueryRepository = uOMConversionQueryRepository;
            _imapper = mapper;
            _mediator = mediator;
        }
    

         public async Task<bool> Handle(DeleteUOMConversionCommand request, CancellationToken cancellationToken)
        {
            // Map the request to the entity
            var uOMConversion = _imapper.Map<InventoryManagement.Domain.Entities.UOMConversion>(request);

            // Perform the delete operation
            var isDeleted = await _iUOMConversionCommandRepository.DeleteAsync(request.Id,  uOMConversion);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: uOMConversion.Id.ToString(),
                actionName: uOMConversion.IsDeleted.ToString(),
                details: $"UOM Conversion with ID {uOMConversion.Id} was deleted.",
                module: "UOM Conversion"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            
             return isDeleted ? true : throw new ExceptionRules("UOM Conversion deletion failed.");
                
            
        }
        
    }
}