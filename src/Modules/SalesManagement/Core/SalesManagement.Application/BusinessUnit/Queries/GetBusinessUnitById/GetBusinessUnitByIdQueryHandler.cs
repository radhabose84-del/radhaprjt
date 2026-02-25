
using Contracts.Common;
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;

namespace SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitById
{
    public class GetBusinessUnitByIdQueryHandler : IRequestHandler<GetBusinessUnitByIdQuery, ApiResponseDTO<BusinessUnitDto>>
    {
        private readonly IBusinessUnitQueryRepository _queryRepository;

        public GetBusinessUnitByIdQueryHandler(IBusinessUnitQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<BusinessUnitDto>> Handle(GetBusinessUnitByIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByIdAsync(request.Id);

            if (data == null)
                throw new EntityNotFoundException("Business Unit not found");

            return new ApiResponseDTO<BusinessUnitDto>
            {
                IsSuccess = true,
                Message = "Business Unit retrieved successfully",
                Data = data
            };
        }
    }
}
