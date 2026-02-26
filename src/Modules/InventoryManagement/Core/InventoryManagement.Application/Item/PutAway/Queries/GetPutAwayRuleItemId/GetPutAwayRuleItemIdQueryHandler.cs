using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleItemId
{
    public class GetPutAwayRuleItemIdQueryHandler : IRequestHandler<GetPutAwayRuleItemIdQuery, List<GetPutAwayRuleItemIdDto>>
    {
        private readonly IPutAwayRuleQueryRepository _iputAwayRuleQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPutAwayRuleItemIdQueryHandler(IPutAwayRuleQueryRepository iputAwayRuleQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iputAwayRuleQueryRepository = iputAwayRuleQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<GetPutAwayRuleItemIdDto>> Handle(GetPutAwayRuleItemIdQuery request, CancellationToken cancellationToken)
        {
             // Fetch data from repository
            var result = await _iputAwayRuleQueryRepository.GetPutAwayRuleDetailsAsync(request.ItemIds ?? new List<int>(),request.WarehouseIds ?? new List<int>());

            // Map to DTOs (if needed — if repository already returns DTOs, you can skip this)
            var getPutAways = _mapper.Map<List<GetPutAwayRuleItemIdDto>>(result);

            // Domain Event logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetPutAwayRuleItemIdQuery",
                actionName: getPutAways.Count.ToString(),
                details: $"PutAway details were fetched for ItemIds: {string.Join(",", request.ItemIds ?? new List<int>())}",
                module: "PartyGroup"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return getPutAways;
        }
    }
}