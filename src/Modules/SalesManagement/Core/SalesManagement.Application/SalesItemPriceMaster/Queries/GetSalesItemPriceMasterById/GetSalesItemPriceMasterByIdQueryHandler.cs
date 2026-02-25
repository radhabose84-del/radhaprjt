using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Dto;

namespace SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterById
{
    public class GetSalesItemPriceMasterByIdQueryHandler
        : IRequestHandler<GetSalesItemPriceMasterByIdQuery, ApiResponseDTO<SalesItemPriceMasterDto>>
    {
        private readonly ISalesItemPriceMasterQueryRepository _queryRepository;

        public GetSalesItemPriceMasterByIdQueryHandler(ISalesItemPriceMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<SalesItemPriceMasterDto>> Handle(
            GetSalesItemPriceMasterByIdQuery request,
            CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByIdAsync(request.Id);
            if (data == null)
                throw new EntityNotFoundException($"Sales Item Price Master with Id {request.Id} not found.");

            return new ApiResponseDTO<SalesItemPriceMasterDto>
            {
                IsSuccess = true,
                Message = "Sales Item Price Master retrieved successfully.",
                Data = data
            };
        }
    }
}
