using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMainStoreStock;
using MediatR;

namespace MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStock
{
    public class GetMainStoreStockQueryHandler : IRequestHandler<GetMainStoreStockQuery,List<MainStoresStockDto>>
    {
        private readonly IMainStoreStockQueryRepository _mainStoreStockQueryRepository;  
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMainStoreStockQueryHandler(IMainStoreStockQueryRepository mainStoreStockQueryRepository, IMapper mapper, IMediator mediator)
        {
            _mainStoreStockQueryRepository = mainStoreStockQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<MainStoresStockDto>> Handle(GetMainStoreStockQuery request, CancellationToken cancellationToken)
        {
            var result = await _mainStoreStockQueryRepository.GetStockDetails(request.OldUnitcode,request.GroupCode);
            var substores = _mapper.Map<List<MainStoresStockDto>>(result);
             //Domain Event
                var domainEvent = new Domain.Events.AuditLogsDomainEvent(
                    actionDetail: "GetAllStock",
                    actionCode: "GetMainStoreStockQuery",        
                    actionName: substores.Count.ToString(),
                    details: $"Stock details was fetched.",
                    module:"MainStoresStock"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return substores;
        }
    }
}