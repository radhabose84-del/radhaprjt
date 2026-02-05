using AutoMapper;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.BankAccount.Query.GetBankAccountById
{
    public class GetBankAccountByIdQueryHandler : IRequestHandler<GetBankAccountByIdQuery, BankAccountDto?>
    {
        private readonly IBankAccountQueryRepository _read;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; // ← added

        public GetBankAccountByIdQueryHandler(IBankAccountQueryRepository read, IMapper mapper, IMediator mediator)
        {
            _read = read;
            _mapper = mapper;
            _mediator = mediator; // ← added
        }

        public async Task<BankAccountDto?> Handle(GetBankAccountByIdQuery r, CancellationToken ct)
        {
            var entity = await _read.GetByIdAsync(r.Id, ct);
            var dto = entity is null ? null : _mapper.Map<BankAccountDto>(entity);

            // 🔹 Audit event
            var ev = new AuditLogsDomainEvent(
                actionDetail: "Query",
                actionCode: "GetById",
                actionName: dto?.BankId.ToString() ?? "BankAccount",
                details: entity is null
                    ? $"Bank account (Id={r.Id}) not found."
                    : $"Fetched bank account (Id={r.Id}).",
                module: "BankAccount"
            );
            await _mediator.Publish(ev, ct);

            return dto;
        }
    }
}
