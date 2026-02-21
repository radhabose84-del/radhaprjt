#nullable disable

using Contracts.Common;
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;

namespace SalesManagement.Application.BusinessUnit.Queries.GetAllBusinessUnit
{
    public class GetAllBusinessUnitQueryHandler : IRequestHandler<GetAllBusinessUnitQuery, ApiResponseDTO<List<BusinessUnitDto>>>
    {
        private readonly IBusinessUnitQueryRepository _queryRepository;

        public GetAllBusinessUnitQueryHandler(IBusinessUnitQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<List<BusinessUnitDto>>> Handle(GetAllBusinessUnitQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm);

            return new ApiResponseDTO<List<BusinessUnitDto>>
            {
                IsSuccess = true,
                Message = "Business Units retrieved successfully",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
