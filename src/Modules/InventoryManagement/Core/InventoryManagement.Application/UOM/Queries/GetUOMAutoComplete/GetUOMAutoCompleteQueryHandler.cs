#nullable disable
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UOM.Queries.GetUOMAutoComplete
{
    public class GetUOMAutoCompleteQueryHandler  : IRequestHandler<GetUOMAutoCompleteQuery, ApiResponseDTO<List<UOMAutoCompleteDto>>>
    {
           private readonly IUOMQueryRepository _uomQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetUOMAutoCompleteQueryHandler(IUOMQueryRepository uomQueryRepository,IMapper mapper,IMediator mediator)
        {
            _uomQueryRepository = uomQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<UOMAutoCompleteDto>>> Handle(GetUOMAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _uomQueryRepository.GetUOM(request.SearchPattern);
            if (result is null || result.Count is 0)
            {
                return new ApiResponseDTO<List<UOMAutoCompleteDto>>
                {
                    IsSuccess = false,
                    Message = "No UOM found matching the search pattern."
                };
            }
              var uom = _mapper.Map<List<UOMAutoCompleteDto>>(result);
              //Domain Event
                 var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "GetUOMAutoComplete",
                     actionCode: "",
                     actionName: "",
                     details: $"UOM details was fetched.",
                     module:"UOM"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<UOMAutoCompleteDto>> { IsSuccess = true, Message = "Success", Data = uom };  
        }
    }
}