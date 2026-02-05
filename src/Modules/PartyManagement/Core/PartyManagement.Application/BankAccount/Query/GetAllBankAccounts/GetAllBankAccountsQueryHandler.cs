using AutoMapper;
using PartyManagement.Application.BankAccount;
using PartyManagement.Application.BankAccount.Query.GetBankAccountsPaged;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.BankAccount.Query.GetAllBankAccounts
{
    public class GetAllBankAccountsQueryHandler : IRequestHandler<GetAllBankAccountsQuery, (IReadOnlyList<BankAccountDto> Items, int Total)>
    {
        private readonly IBankAccountQueryRepository _read;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; // ← added

        public GetAllBankAccountsQueryHandler(IBankAccountQueryRepository read, IMapper mapper, IMediator mediator)
        {
            _read = read;
            _mapper = mapper;
            _mediator = mediator; // ← added
        }

        public async Task<(IReadOnlyList<BankAccountDto> Items, int Total)> Handle(GetAllBankAccountsQuery r, CancellationToken ct)
        {
            var (entities, total) = await _read.GetAllAsync(r.PageNumber, r.PageSize, r.Search,r.BankId, ct);
            var items = entities.Select(e => _mapper.Map<BankAccountDto>(e)).ToList();

            // 🔹 Audit event
            var ev = new AuditLogsDomainEvent(
                actionDetail: "Query",
                actionCode: "GetAll",
                actionName: items.Count.ToString(),
                details: $"Fetched bank accounts page={r.PageNumber}, size={r.PageSize}, search='{r.Search}'. Total={total}.",
                module: "BankAccount"
            );
            await _mediator.Publish(ev, ct);

            return (items, total);
        }
    }
}
