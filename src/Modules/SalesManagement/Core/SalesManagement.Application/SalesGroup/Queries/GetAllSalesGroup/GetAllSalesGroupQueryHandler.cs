#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Dto;

namespace SalesManagement.Application.SalesGroup.Queries.GetAllSalesGroup
{
    public class GetAllSalesGroupQueryHandler : IRequestHandler<GetAllSalesGroupQuery, ApiResponseDTO<List<SalesGroupDto>>>
    {
        private readonly ISalesGroupQueryRepository _queryRepository;

        public GetAllSalesGroupQueryHandler(ISalesGroupQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<List<SalesGroupDto>>> Handle(GetAllSalesGroupQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<SalesGroupDto>>
            {
                IsSuccess = true,
                Message = "Sales Groups retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
