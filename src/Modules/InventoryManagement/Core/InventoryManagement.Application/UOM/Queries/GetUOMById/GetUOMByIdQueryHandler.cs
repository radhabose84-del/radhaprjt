using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UOM.Queries.GetUOMById
{
    public class GetUOMByIdQueryHandler  : IRequestHandler<GetUOMByIdQuery, ApiResponseDTO<UOMDto>>
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
        public async Task<ApiResponseDTO<UOMDto>> Handle(GetUOMByIdQuery request, CancellationToken cancellationToken)
        {
           var result = await _uomQueryRepository.GetByIdAsync(request.Id);
            if (result is null)
            {
                return new ApiResponseDTO<UOMDto>
                {
                    IsSuccess = false,
                    Message = "UOMId not found"
                };
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
          return new ApiResponseDTO<UOMDto> { IsSuccess = true, Message = "Success", Data = location };
        }
        
    }
}