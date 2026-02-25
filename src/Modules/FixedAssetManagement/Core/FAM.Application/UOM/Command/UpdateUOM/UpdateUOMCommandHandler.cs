#nullable disable
using AutoMapper;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.UOM.Command.UpdateUOM
{
    public class UpdateUOMCommandHandler : IRequestHandler<UpdateUOMCommand, bool>
    {
         private readonly IUOMCommandRepository _uomCommandRepository;
        private readonly IUOMQueryRepository _uomQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public UpdateUOMCommandHandler(IUOMCommandRepository uomCommandRepository,IUOMQueryRepository uomQueryRepository,IMapper mapper,IMediator mediator)
        {
           _uomCommandRepository = uomCommandRepository;
           _uomQueryRepository = uomQueryRepository;
           _mapper = mapper;
           _mediator = mediator; 
        }
        public async Task<bool> Handle(UpdateUOMCommand request, CancellationToken cancellationToken)
        {
            var existinguom = await _uomQueryRepository.GetByUOMNameAsync(request.UOMName, request.Id);

                if (existinguom != null)
                {
                    throw new ValidationException("UOM already exists");
                    
                }

            if (request.IsActive == 0) // Inactive
            {
                var linked = await _uomQueryRepository.IsUomLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }
            // Check for duplicate GroupName or SortOrder
            var (isNameDuplicate, isSortOrderDuplicate) = await _uomCommandRepository
                                .CheckForDuplicatesAsync(request.UOMName, request.SortOrder, request.Id);

        if (isNameDuplicate || isSortOrderDuplicate)
        {
            string errorMessage = isNameDuplicate && isSortOrderDuplicate
            ? "Both UOMName and Sort Order already exist."
            : isNameDuplicate
            ? "UOM with the same UOMName already exists."
            : "UOM with the same Sort Order already exists.";

        throw new ValidationException(errorMessage);
           
        }

                 var uom  = _mapper.Map<FAM.Domain.Entities.UOM>(request);
         
                var uomresult = await _uomCommandRepository.UpdateAsync(uom);

                
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: uom.Code,
                        actionName: uom.UOMName,
                        details: $"UOM '{uom.Id}' was updated.",
                        module:"UOM"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken); 
              
                if(uomresult)
                {
                    
                    return uomresult;
                }
            throw new Exception("UOM not updated.");
                
        }
    }
}