
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;

namespace SalesManagement.Application.SalesSegment.Queries.GetAllSalesSegment
{
    public class GetAllSalesSegmentQueryHandler : IRequestHandler<GetAllSalesSegmentQuery, ApiResponseDTO<List<SalesSegmentDto>>>
    {
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly ICurrencyLookup _currencyLookup;

        public GetAllSalesSegmentQueryHandler(
            ISalesSegmentQueryRepository queryRepository,
            ICurrencyLookup currencyLookup)
        {
            _queryRepository = queryRepository;
            _currencyLookup = currencyLookup;
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

            return new ApiResponseDTO<List<SalesSegmentDto>>
            {
                IsSuccess = true,
                Message = "Sales Segments retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
