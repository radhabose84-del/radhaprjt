using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;

namespace SalesManagement.Application.SalesOrganisation.Queries.GetAllSalesOrganisation
{
    public class GetAllSalesOrganisationQueryHandler : IRequestHandler<GetAllSalesOrganisationQuery, ApiResponseDTO<List<SalesOrganisationDto>>>
    {
        private readonly ISalesOrganisationQueryRepository _queryRepository;

        public GetAllSalesOrganisationQueryHandler(ISalesOrganisationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<List<SalesOrganisationDto>>> Handle(GetAllSalesOrganisationQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<SalesOrganisationDto>>
            {
                IsSuccess = true,
                Message = "Sales Organisations retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
