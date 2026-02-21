#nullable disable

using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;

namespace SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentById
{
    public class GetSalesSegmentByIdQueryHandler : IRequestHandler<GetSalesSegmentByIdQuery, ApiResponseDTO<SalesSegmentDto>>
    {
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly ICurrencyLookup _currencyLookup;

        public GetSalesSegmentByIdQueryHandler(
            ISalesSegmentQueryRepository queryRepository,
            ICurrencyLookup currencyLookup)
        {
            _queryRepository = queryRepository;
            _currencyLookup = currencyLookup;
        }

        public async Task<ApiResponseDTO<SalesSegmentDto>> Handle(GetSalesSegmentByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _queryRepository.GetByIdAsync(request.Id);

            if (dto == null)
            {
                return new ApiResponseDTO<SalesSegmentDto>
                {
                    IsSuccess = false,
                    Message = "Sales Segment not found.",
                    Data = null
                };
            }

            // Populate currency name via lookup
            if (dto.CurrencyId.HasValue)
            {
                var currencies = await _currencyLookup.GetByIdsAsync(new[] { dto.CurrencyId.Value }, cancellationToken);
                var currency = currencies.FirstOrDefault();
                dto.CurrencyName = currency?.Name;
            }

            return new ApiResponseDTO<SalesSegmentDto>
            {
                IsSuccess = true,
                Message = "Sales Segment retrieved successfully.",
                Data = dto
            };
        }
    }
}
