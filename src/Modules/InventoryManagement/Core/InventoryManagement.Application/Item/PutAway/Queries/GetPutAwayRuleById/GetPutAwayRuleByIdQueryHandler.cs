using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleById
{
    public class GetPutAwayRuleByIdQueryHandler : IRequestHandler<GetPutAwayRuleByIdQuery, PutAwayRuleDetailDto>
    {
        private readonly IPutAwayRuleQueryRepository _queryRepo;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetPutAwayRuleByIdQueryHandler(IPutAwayRuleQueryRepository queryRepo, IMediator mediator, IMapper mapper)
        {
            _queryRepo = queryRepo;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<PutAwayRuleDetailDto> Handle(GetPutAwayRuleByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepo.GetByIdAsync(request.Id, cancellationToken);
            if (result is null)
                throw new EntityNotFoundException(nameof(PutAwayRuleDetailDto), request.Id);

            var dto = _mapper.Map<PutAwayRuleDetailDto>(result);

            // 📘 Audit
            var ev = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetPutAwayRuleById",
                actionName: dto.Id.ToString(),
                details: $"Put-away rule {dto.Id} was fetched.",
                module: "PutAway"
            );
            await _mediator.Publish(ev, cancellationToken);

            return dto;
        }
    }
}
