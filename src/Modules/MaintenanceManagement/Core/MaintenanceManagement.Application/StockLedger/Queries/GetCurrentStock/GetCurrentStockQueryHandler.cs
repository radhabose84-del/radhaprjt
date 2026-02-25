#nullable disable
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IStcokLedger;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock
{
    public class GetCurrentStockQueryHandler : IRequestHandler<GetCurrentStockQuery, ApiResponseDTO<CurrentStockDto>>
    {
        private readonly IStockLedgerQueryRepository _stockLedgerQueryRepository;
        private readonly IMapper _mapper;        
        private readonly IMediator _mediator; 
        
        public GetCurrentStockQueryHandler(IStockLedgerQueryRepository stockLedgerQueryRepository, IMapper mapper, IMediator mediator)
        {
            _stockLedgerQueryRepository = stockLedgerQueryRepository;
            _mapper = mapper;            
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<CurrentStockDto>> Handle(GetCurrentStockQuery request, CancellationToken cancellationToken)
        {
            var result = await _stockLedgerQueryRepository.GetSubStoresCurrentStock(request.OldUnitId, request.ItemCode,request.DepartmentId);

            if (result is null)
            {
                return new ApiResponseDTO<CurrentStockDto>
                {
                    IsSuccess = false,
                    Message = $"No Stock found for ItemCode: {request.ItemCode}."
                };
            }

            // ✅ AutoMapper used for single object
            var substoresStock = _mapper.Map<CurrentStockDto>(result);

            // Domain event logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetCurrentStockQuery",
                actionCode: "SubStoresStock",
                actionName: request.ItemCode.ToString(),
                details: $"SubStores CurrentStock found for ItemCode {request.ItemCode} were fetched.",
                module: "SubStoresStock"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<CurrentStockDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = substoresStock
            };
        }

    }
}