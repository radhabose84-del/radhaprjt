using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.UOM.Queries.GetUOMs
{
    public class GetUOMHandlerQuery : IRequestHandler<GetUOMQuery, ApiResponseDTO<List<UOMDto>>>
    {
        private readonly IUOMQueryRepository _uomQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetUOMHandlerQuery(IUOMQueryRepository uomQueryRepository,IMapper mapper,IMediator mediator)
        {
            _uomQueryRepository = uomQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<UOMDto>>> Handle(GetUOMQuery request, CancellationToken cancellationToken)
        {
            var (uoms, totalCount) = await _uomQueryRepository.GetAllUOMAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var uomList = _mapper.Map<List<UOMDto>>(uoms);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetUOMs",
                    actionCode: "",        
                    actionName: "",
                    details: $"UOM details was fetched.",
                    module:"UOM"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<UOMDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = uomList ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}