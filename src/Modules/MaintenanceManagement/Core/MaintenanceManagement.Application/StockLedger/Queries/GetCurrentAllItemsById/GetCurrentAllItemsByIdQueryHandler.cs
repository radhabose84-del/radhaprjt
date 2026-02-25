#nullable disable
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IStcokLedger;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentAllItemsById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStockItemsById
{
    public class GetCurrentAllItemsByIdQueryHandler : IRequestHandler<GetCurrentAllItemsByIdQuery,ApiResponseDTO<List<StockItemCodeDto>>>
    {
        private readonly IStockLedgerQueryRepository _stockLedgerQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCurrentAllItemsByIdQueryHandler(IStockLedgerQueryRepository stockLedgerQueryRepository, IMapper mapper, IMediator mediator)
        {
            _stockLedgerQueryRepository = stockLedgerQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<StockItemCodeDto>>> Handle(GetCurrentAllItemsByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _stockLedgerQueryRepository.GetAllItemCodes(request.OldUnitcode,request.DepartmentId);
            var costCenters = _mapper.Map<List<StockItemCodeDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetCurrentStockItemsByIdQuery",        
                    actionName: costCenters.Count.ToString(),
                    details: $"Item details was fetched.",
                    module:"StockItemMaster"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<StockItemCodeDto>> { IsSuccess = true, Message = "Success", Data = costCenters };
        }
    }
}