#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;

namespace SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeById
{
    public class GetSalesOfficeByIdQueryHandler : IRequestHandler<GetSalesOfficeByIdQuery, ApiResponseDTO<SalesOfficeDto>>
    {
        private readonly ISalesOfficeQueryRepository _queryRepository;

        public GetSalesOfficeByIdQueryHandler(ISalesOfficeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<SalesOfficeDto>> Handle(GetSalesOfficeByIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByIdAsync(request.Id);
            if (data == null)
                throw new EntityNotFoundException($"Sales Office with Id {request.Id} not found.");

            return new ApiResponseDTO<SalesOfficeDto>
            {
                IsSuccess = true,
                Message = "Sales Office retrieved successfully.",
                Data = data
            };
        }
    }
}
