using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;

namespace SalesManagement.Application.SalesOffice.Queries.GetAllSalesOffice
{
    public class GetAllSalesOfficeQueryHandler : IRequestHandler<GetAllSalesOfficeQuery, ApiResponseDTO<List<SalesOfficeDto>>>
    {
        private readonly ISalesOfficeQueryRepository _queryRepository;

        public GetAllSalesOfficeQueryHandler(ISalesOfficeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<List<SalesOfficeDto>>> Handle(GetAllSalesOfficeQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<SalesOfficeDto>>
            {
                IsSuccess = true,
                Message = "Sales Offices retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
