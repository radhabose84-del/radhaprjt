using PartyManagement.Application.Common.Interfaces.IBankMaster;
using MediatR;

namespace PartyManagement.Application.BankMaster.Queries.GetBankMastersAutocomplete;


public class GetBankMastersAutocompleteHandler
    : IRequestHandler<GetBankMastersAutocompleteQuery, IReadOnlyList<AutocompleteItemDto>>
{
    private readonly IBankMasterQueryRepository _qry;
    public GetBankMastersAutocompleteHandler(IBankMasterQueryRepository qry) => _qry = qry;

    public async Task<IReadOnlyList<AutocompleteItemDto>> Handle(GetBankMastersAutocompleteQuery r, CancellationToken ct)
    {
        var list = await _qry.GetAutocompleteAsync(r.Search, ct);
        return list.Select(b => new AutocompleteItemDto(b.Id, $"{b.BankName} ({b.BankCode})", b.BankCode)).ToList();
    }
}
