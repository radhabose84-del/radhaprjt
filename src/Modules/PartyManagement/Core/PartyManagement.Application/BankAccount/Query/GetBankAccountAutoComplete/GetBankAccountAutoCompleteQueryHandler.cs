// PartyManagement.Application/BankAccount/Query/GetBankAutocomplete/GetBankAutocompleteQueryHandler.cs
using PartyManagement.Application.BankAccount.Query.GetBankAutocomplete;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.BankAccount.Query.GetBankAutocomplete
{
    public sealed class GetBankAccountAutoCompleteQueryHandler
        : IRequestHandler<GetBankAccountAutoCompleteQuery, IReadOnlyList<BankLookupDto>>
    {
        private readonly IBankAccountQueryRepository _read;
        private readonly IMediator _mediator;

        public GetBankAccountAutoCompleteQueryHandler(IBankAccountQueryRepository read, IMediator mediator)
        {
            _read = read;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<BankLookupDto>> Handle(GetBankAccountAutoCompleteQuery r, CancellationToken ct)
        {
            var term = r.Term ?? string.Empty;

            var items = await _read.AutocompleteAsync(term, ct);

            // 🔹 Audit event (same pattern you used)
            var ev = new AuditLogsDomainEvent(
                actionDetail: "Query",
                actionCode: "Autocomplete",
                actionName: items.Count.ToString(),
                details: $"BankAccount autocomplete term='{term}'. Returned={items.Count}.",
                module: "BankAccount"
            );
            await _mediator.Publish(ev, ct);

            return items;
        }
    }
}
