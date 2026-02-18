#nullable disable
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.UOM.Queries.GetUOMs;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.UOM.Queries.GetUOMTypeAutoComplete
{
    public class GetUOMTypeAutoCompleteHandlerQuery : IRequestHandler<GetUOMTypeAutoCompleteQuery, List<UOMTypeAutoCompleteDto>>
    {
        private readonly IUOMQueryRepository _uomQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetUOMTypeAutoCompleteHandlerQuery(IUOMQueryRepository uomQueryRepository,IMapper mapper,IMediator mediator)
        {
            _uomQueryRepository = uomQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;   
        }

        public async Task<List<UOMTypeAutoCompleteDto>> Handle(GetUOMTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _uomQueryRepository.GetUOMType(request.SearchPattern);

            if (result is null || result.Count == 0)
            {
                throw new ValidationException("No UOMType found matching the search pattern.");
              
            }

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetUOMTypeAutoComplete",
                actionCode: "",
                actionName: "",
                details: $"UOMType details were fetched.",
                module: "UOMType"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return  result;
            
        }
    }
}