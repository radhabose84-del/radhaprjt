using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;

namespace SalesManagement.Application.SalesChannel.Queries.GetSalesChannelById
{
    public class GetSalesChannelByIdQueryHandler : IRequestHandler<GetSalesChannelByIdQuery, ApiResponseDTO<SalesChannelDto>>
    {
        private readonly ISalesChannelQueryRepository _queryRepository;

        public GetSalesChannelByIdQueryHandler(ISalesChannelQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<SalesChannelDto>> Handle(GetSalesChannelByIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByIdAsync(request.Id);
            if (data == null)
                throw new EntityNotFoundException($"Sales Channel with Id {request.Id} not found.");

            return new ApiResponseDTO<SalesChannelDto>
            {
                IsSuccess = true,
                Message = "Sales Channel retrieved successfully.",
                Data = data
            };
        }
    }
}
