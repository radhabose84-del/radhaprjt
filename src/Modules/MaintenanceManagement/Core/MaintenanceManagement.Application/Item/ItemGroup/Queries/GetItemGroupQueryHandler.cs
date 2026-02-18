#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IItem;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Item.ItemGroup.Queries
{
    public class GetItemGroupQueryHandler :  IRequestHandler<GetItemGroupQuery,List<GetItemGroupDto>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IItemQueryRepository _itemQueryRepository;

        public GetItemGroupQueryHandler(IMapper mapper, IMediator mediator, IItemQueryRepository itemQueryRepository)
        {
            _mapper = mapper;
            _mediator = mediator;
            _itemQueryRepository = itemQueryRepository;
        }

        public async Task<List<GetItemGroupDto>> Handle(GetItemGroupQuery request, CancellationToken cancellationToken)
        {
             var result = await _itemQueryRepository.GetGroupCodes(request.OldUnitId);
            var itemgroupcode  = _mapper.Map<List<GetItemGroupDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetItemGroupQuery",        
                    actionName: "ItemGroupCode",
                    details: $"ItemGroupCode details was fetched.",
                    module:"ItemGroupCode"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return itemgroupcode;
        }
      
    }
}