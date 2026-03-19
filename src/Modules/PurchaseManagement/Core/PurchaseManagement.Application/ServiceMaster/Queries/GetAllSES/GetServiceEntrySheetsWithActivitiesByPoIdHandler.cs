using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetAllSES
{
    public class GetServiceEntrySheetsWithActivitiesByPoIdHandler : IRequestHandler<GetServiceEntrySheetsWithActivitiesByPoIdQuery, ApiResponseDTO<List<ServiceEntrySheetWithActivitiesDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;

        public GetServiceEntrySheetsWithActivitiesByPoIdHandler(IMapper mapper, IMediator mediator, IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository)
        {
            _mapper = mapper;
            _mediator = mediator;
            _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
        }

        public async Task<ApiResponseDTO<List<ServiceEntrySheetWithActivitiesDto>>> Handle(
            GetServiceEntrySheetsWithActivitiesByPoIdQuery request,
            CancellationToken cancellationToken)
        {
            var sesList = (await _servicePurchaseOrderQueryRepository.GetByPurchaseOrderIdAsync(request.PurchaseOrderId, cancellationToken))
                .ToList();

            if (!sesList.Any())
            {
                return new ApiResponseDTO<List<ServiceEntrySheetWithActivitiesDto>>
                {
                    IsSuccess = false,
                    Message = "No Service Entry Sheets found for this Purchase Order.",
                    Data = new List<ServiceEntrySheetWithActivitiesDto>()
                };
            }

            return new ApiResponseDTO<List<ServiceEntrySheetWithActivitiesDto>>
            {
                IsSuccess = true,
                Message = "Service Entry Sheets with activities retrieved successfully.",
                Data = sesList
            };
        }
    }
}
