using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesSegment.Queries.GetAllSalesSegment
{
    public class GetAllSalesSegmentQueryHandler : IRequestHandler<GetAllSalesSegmentQuery, ApiResponseDTO<List<SalesSegmentDto>>>
    {
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesSegmentQueryHandler(
            ISalesSegmentQueryRepository queryRepository,
            ICurrencyLookup currencyLookup,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _currencyLookup = currencyLookup;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesSegmentDto>>> Handle(GetAllSalesSegmentQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm);

            // Populate currency names via lookup
            if (data != null && data.Any())
            {
                var currencyIds = data.Where(x => x.CurrencyId.HasValue)
                    .Select(x => x.CurrencyId!.Value)
                    .Distinct()
                    .ToList();

                if (currencyIds.Any())
                {
                    var currencies = await _currencyLookup.GetByIdsAsync(currencyIds, cancellationToken);

                    foreach (var item in data.Where(x => x.CurrencyId.HasValue))
                    {
                        var currency = currencies.FirstOrDefault(c => c.CurrencyId == item.CurrencyId!.Value);
                        item.CurrencyName = currency?.Name;
                    }
                }
            }

            var salesSegmentDtos = _mapper.Map<List<SalesSegmentDto>>(data);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesSegmentQuery",
                actionCode: "Get",
                actionName: (data?.Count ?? 0).ToString(),
                details: "SalesSegment details were fetched.",
                module: "SalesSegment"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesSegmentDto>>
            {
                IsSuccess = true,
                Message = "Sales Segments retrieved successfully.",
                Data = salesSegmentDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
