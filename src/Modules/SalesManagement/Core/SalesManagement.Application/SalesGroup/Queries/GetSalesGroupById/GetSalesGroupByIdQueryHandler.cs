using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Dto;

namespace SalesManagement.Application.SalesGroup.Queries.GetSalesGroupById
{
    public class GetSalesGroupByIdQueryHandler : IRequestHandler<GetSalesGroupByIdQuery, ApiResponseDTO<SalesGroupDto>>
    {
        private readonly ISalesGroupQueryRepository _queryRepository;

        public GetSalesGroupByIdQueryHandler(ISalesGroupQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<SalesGroupDto>> Handle(GetSalesGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByIdAsync(request.Id);
            if (data == null)
                throw new EntityNotFoundException($"Sales Group with Id {request.Id} not found.");

            return new ApiResponseDTO<SalesGroupDto>
            {
                IsSuccess = true,
                Message = "Sales Group retrieved successfully.",
                Data = data
            };
        }
    }
}
