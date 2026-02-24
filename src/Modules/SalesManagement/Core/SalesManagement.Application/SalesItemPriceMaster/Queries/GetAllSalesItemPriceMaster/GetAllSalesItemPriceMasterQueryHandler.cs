#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Dto;

namespace SalesManagement.Application.SalesItemPriceMaster.Queries.GetAllSalesItemPriceMaster
{
    public class GetAllSalesItemPriceMasterQueryHandler
        : IRequestHandler<GetAllSalesItemPriceMasterQuery, ApiResponseDTO<List<SalesItemPriceMasterDto>>>
    {
        private readonly ISalesItemPriceMasterQueryRepository _queryRepository;

        public GetAllSalesItemPriceMasterQueryHandler(ISalesItemPriceMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<List<SalesItemPriceMasterDto>>> Handle(
            GetAllSalesItemPriceMasterQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<SalesItemPriceMasterDto>>
            {
                IsSuccess = true,
                Message = "Sales Item Price Masters retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
