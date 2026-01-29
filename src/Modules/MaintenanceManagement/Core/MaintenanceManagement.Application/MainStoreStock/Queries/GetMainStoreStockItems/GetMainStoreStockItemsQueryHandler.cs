using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMainStoreStock;
using MediatR;

namespace MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStockItems
{
    public class GetMainStoreStockItemsQueryHandler : IRequestHandler<GetMainStoreStockItemsQuery,List<MainStoresStockItemsDto>>
    {
        private readonly IMainStoreStockQueryRepository _mainStoreStockQueryRepository;  
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMainStoreStockItemsQueryHandler(IMainStoreStockQueryRepository mainStoreStockQueryRepository, IMapper mapper, IMediator mediator)
        {
            _mainStoreStockQueryRepository = mainStoreStockQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<MainStoresStockItemsDto>> Handle(GetMainStoreStockItemsQuery request, CancellationToken cancellationToken)
        {
            var result = await _mainStoreStockQueryRepository.GetStockItemsCodes(request.OldUnitcode,request.GroupCode);
            var substores = _mapper.Map<List<MainStoresStockItemsDto>>(result);
             //Domain Event
                var domainEvent = new Domain.Events.AuditLogsDomainEvent(
                    actionDetail: "GetAllStock",
                    actionCode: "GetMainStoreStockItemsCodesQuery",        
                    actionName: substores.Count.ToString(),
                    details: $"Stock ItemCodes details was fetched.",
                    module:"MainStoresStockItemCodes"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return  substores;
        }
    }
}