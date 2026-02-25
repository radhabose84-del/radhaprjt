using AutoMapper;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.UOM.Command.DeleteUOM
{
    public class DeleteUOMCommandHandler : IRequestHandler<DeleteUOMCommand, bool>
    {
        private readonly IUOMCommandRepository _uomCommandRepository;
        private readonly IUOMQueryRepository _uomQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public DeleteUOMCommandHandler(IUOMCommandRepository uomCommandRepository,IUOMQueryRepository uomQueryRepository, IMediator mediator,IMapper mapper)
        {
            _uomCommandRepository = uomCommandRepository;
            _uomQueryRepository = uomQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }
        public async Task<bool> Handle(DeleteUOMCommand request, CancellationToken cancellationToken)
        {
            var uom  = _mapper.Map<FAM.Domain.Entities.UOM>(request);
            var uomresult = await _uomCommandRepository.DeleteAsync(request.Id, uom);

            var isLinked = await _uomQueryRepository.IsUomLinkedAsync(request.Id);
            if (isLinked)
            {
                 throw new ValidationException("This master is linked with other records and cannot be deleted.");
            }
             

                  //Domain Event  
            var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Delete",
                        actionCode: uom.Id.ToString(),
                        actionName: uom.Id.ToString(),
                        details: $"UOM '{uom.Id}' was deleted.",
                        module:"UOM"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken);  

                 if(uomresult)
                {
                    return uomresult;
                }
                throw new ValidationException("UOM not deleted.");
                
        }
    }
}