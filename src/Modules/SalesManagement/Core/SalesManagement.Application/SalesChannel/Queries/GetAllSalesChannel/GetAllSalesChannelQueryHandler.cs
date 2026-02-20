#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;

namespace SalesManagement.Application.SalesChannel.Queries.GetAllSalesChannel
{
    public class GetAllSalesChannelQueryHandler : IRequestHandler<GetAllSalesChannelQuery, ApiResponseDTO<List<SalesChannelDto>>>
    {
        private readonly ISalesChannelQueryRepository _queryRepository;

        public GetAllSalesChannelQueryHandler(ISalesChannelQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<List<SalesChannelDto>>> Handle(GetAllSalesChannelQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<SalesChannelDto>>
            {
                IsSuccess = true,
                Message = "Sales Channels retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
