#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;

namespace SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationById
{
    public class GetSalesOrganisationByIdQueryHandler : IRequestHandler<GetSalesOrganisationByIdQuery, ApiResponseDTO<SalesOrganisationDto>>
    {
        private readonly ISalesOrganisationQueryRepository _queryRepository;

        public GetSalesOrganisationByIdQueryHandler(ISalesOrganisationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<SalesOrganisationDto>> Handle(GetSalesOrganisationByIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByIdAsync(request.Id);
            if (data == null)
                throw new EntityNotFoundException($"Sales Organisation with Id {request.Id} not found.");

            return new ApiResponseDTO<SalesOrganisationDto>
            {
                IsSuccess = true,
                Message = "Sales Organisation retrieved successfully.",
                Data = data
            };
        }
    }
}
