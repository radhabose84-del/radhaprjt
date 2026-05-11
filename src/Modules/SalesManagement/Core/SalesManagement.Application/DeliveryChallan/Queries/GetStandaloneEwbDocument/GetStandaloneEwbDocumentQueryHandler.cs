using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetStandaloneEwbDocument
{
    public sealed class GetStandaloneEwbDocumentQueryHandler
        : IRequestHandler<GetStandaloneEwbDocumentQuery, ApiResponseDTO<StandaloneEwbDocumentDto>>
    {
        private readonly IDeliveryChallanQueryRepository _repo;

        public GetStandaloneEwbDocumentQueryHandler(IDeliveryChallanQueryRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponseDTO<StandaloneEwbDocumentDto>> Handle(
            GetStandaloneEwbDocumentQuery request, CancellationToken cancellationToken)
        {
            var doc = await _repo.GetStandaloneEwbDocumentAsync(request.DeliveryChallanId, cancellationToken);

            if (doc == null)
                return new ApiResponseDTO<StandaloneEwbDocumentDto>
                {
                    IsSuccess = false,
                    Message = $"No e-Waybill found for Delivery Challan {request.DeliveryChallanId}."
                };

            return new ApiResponseDTO<StandaloneEwbDocumentDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = doc
            };
        }
    }
}
