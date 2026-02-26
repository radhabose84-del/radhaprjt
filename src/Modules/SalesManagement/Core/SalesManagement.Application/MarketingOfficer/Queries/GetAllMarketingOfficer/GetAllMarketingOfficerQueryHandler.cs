using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MarketingOfficer.Queries.GetAllMarketingOfficer
{
    public class GetAllMarketingOfficerQueryHandler : IRequestHandler<GetAllMarketingOfficerQuery, ApiResponseDTO<List<MarketingOfficerDto>>>
    {
        private readonly IMarketingOfficerQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllMarketingOfficerQueryHandler(IMarketingOfficerQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<MarketingOfficerDto>>> Handle(GetAllMarketingOfficerQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<MarketingOfficerDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllMarketingOfficerQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "MarketingOfficer details were fetched.",
                module: "MarketingOfficer"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<MarketingOfficerDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
