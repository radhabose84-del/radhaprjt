using AutoMapper;
using InventoryManagement.Application.Common.Exceptions;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Domain.Entities.Item.PutAway;
using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule
{
    public sealed class CreatePutAwayRuleCommandHandler : IRequestHandler<CreatePutAwayRuleCommand, int>
    {
        private readonly IPutAwayRuleCommandRepository _cmd;
        private readonly IMapper _mapper;

        public CreatePutAwayRuleCommandHandler(IPutAwayRuleCommandRepository cmd, IMapper mapper)
        { _cmd = cmd; _mapper = mapper; }

        public async Task<int> Handle(CreatePutAwayRuleCommand request, CancellationToken ct)
        {
            var m = request.Body;

            if (await _cmd.ExistsScopeAsync(m.UnitId, m.WarehouseId, m.ItemGroupId, m.ItemCategoryId, m.ItemId, null, ct))
                throw new EntityAlreadyExistsException("Put-away rule already exists for this scope.");

            var e = _mapper.Map<PutAwayRule>(m);
            e.Strategies = m.Strategies
                .OrderBy(s => s.PriorityId)
                .Select(s => _mapper.Map<PutAwayStrategy>(s))
                .ToList();

            await _cmd.AddAsync(e, ct);
            await _cmd.SaveAsync(ct);
            return e.Id;
        }
    }
}