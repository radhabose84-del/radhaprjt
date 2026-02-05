using MediatR;

namespace PartyManagement.Application.BankAccount.Query.GetBankAutocomplete;

public sealed record GetBankAccountAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<BankLookupDto>>;

public sealed class BankLookupDto
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = default!;
    public string AccountHolderName { get; set; } = default!;
    public string BankName { get; set; } = default!;
    public string IFSCCode { get; set; } = default!;
    public string SWIFTCode { get; set; } = default!;
}
