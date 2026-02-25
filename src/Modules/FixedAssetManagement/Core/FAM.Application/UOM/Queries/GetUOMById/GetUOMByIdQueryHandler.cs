using AutoMapper;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.UOM.Queries.GetUOMs;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.UOM.Queries.GetUOMById
{
    public class GetUOMByIdQueryHandler : IRequestHandler<GetUOMByIdQuery, UOMDto>
    {
         private readonly IUOMQueryRepository _uomQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetUOMByIdQueryHandler(IUOMQueryRepository uomQueryRepository,IMapper mapper,IMediator mediator)
        {
            _uomQueryRepository = uomQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<UOMDto> Handle(GetUOMByIdQuery request, CancellationToken cancellationToken)
        {
           var result = await _uomQueryRepository.GetByIdAsync(request.Id);
            if (result is null)
            {
                throw new ValidationException("UOMId not found");
               
            }  
           var location = _mapper.Map<UOMDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"UOM details {location.Id} was fetched.",
                    module:"UOM"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return location;
        }
    }
}