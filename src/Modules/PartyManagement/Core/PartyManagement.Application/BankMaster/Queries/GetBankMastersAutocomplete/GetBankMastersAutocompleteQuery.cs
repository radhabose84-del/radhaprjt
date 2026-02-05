using PartyManagement.Application.BankMaster;
using MediatR;

namespace PartyManagement.Application.BankMaster.Queries.GetBankMastersAutocomplete;
public record GetBankMastersAutocompleteQuery(string? Search) : IRequest<IReadOnlyList<AutocompleteItemDto>>;