using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMainStoreStock;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MainStoreStock.Queries.GetItemStockbyId
{
    public class GetItemStockbyIdQueryHandler : IRequestHandler<GetItemStockbyIdQuery,MainStoreItemStockDto>
    {
        private readonly IMainStoreStockQueryRepository _imainStoreStockQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetItemStockbyIdQueryHandler(IMainStoreStockQueryRepository imainStoreStockQueryRepository, IMapper mapper, IMediator mediator)
        {
            _imainStoreStockQueryRepository = imainStoreStockQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MainStoreItemStockDto> Handle(GetItemStockbyIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _imainStoreStockQueryRepository.GetByItemCodeIdAsync(request.OldUnitcode, request.ItemCode);
           
            // Map a single entity
            var itemStockDto = _mapper.Map<MainStoreItemStockDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "GetByItemCodeIdAsync",        
                    actionName: itemStockDto.StockQty.ToString(),
                    details: $"Stock Item details {itemStockDto.StockQty} was fetched.",
                    module:"MainstoreStockItemsFetched"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return itemStockDto;
        }
    }
}