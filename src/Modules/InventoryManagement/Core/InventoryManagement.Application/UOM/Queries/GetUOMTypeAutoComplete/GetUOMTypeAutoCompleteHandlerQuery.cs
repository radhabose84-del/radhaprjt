#nullable disable
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UOM.Queries.GetUOMTypeAutoComplete
{
    public class GetUOMTypeAutoCompleteHandlerQuery : IRequestHandler<GetUOMTypeAutoCompleteQuery, ApiResponseDTO<List<UOMTypeAutoCompleteDto>>>
    {
        private readonly IUOMQueryRepository _uomQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetUOMTypeAutoCompleteHandlerQuery(IUOMQueryRepository uomQueryRepository, IMapper mapper, IMediator mediator)
        {
            _uomQueryRepository = uomQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<UOMTypeAutoCompleteDto>>> Handle(GetUOMTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _uomQueryRepository.GetUOMType(request.SearchPattern);

            if (result is null || result.Count == 0)
            {
                return new ApiResponseDTO<List<UOMTypeAutoCompleteDto>>
                {
                    IsSuccess = false,
                    Message = "No UOMType found matching the search pattern."
                };
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

            return new ApiResponseDTO<List<UOMTypeAutoCompleteDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = result  // No need for _mapper.Map()
            };

        }
    }
}