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
    public int? OwnerTypeId { get; set; }
    public string? OwnerTypeName { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public int? StateId { get; set; }
    public string? StateName { get; set; }
    public string? Pincode { get; set; }
}
