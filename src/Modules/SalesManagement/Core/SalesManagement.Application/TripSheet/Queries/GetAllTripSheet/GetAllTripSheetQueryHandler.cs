using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.TripSheet.Queries.GetAllTripSheet
{
    public class GetAllTripSheetQueryHandler : IRequestHandler<GetAllTripSheetQuery, ApiResponseDTO<List<TripSheetHeaderDto>>>
    {
        private readonly ITripSheetQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllTripSheetQueryHandler(ITripSheetQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<TripSheetHeaderDto>>> Handle(GetAllTripSheetQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllTripSheetQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "TripSheet details were fetched.",
                module: "TripSheet"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<TripSheetHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
