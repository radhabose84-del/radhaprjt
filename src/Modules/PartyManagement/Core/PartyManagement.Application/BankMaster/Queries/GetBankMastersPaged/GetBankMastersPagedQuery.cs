using MediatR;

namespace PartyManagement.Application.BankMaster.Queries.GetBankMastersPaged;
public record GetBankMastersPagedQuery(int PageNumber, int PageSize, string? Search)
    : IRequest<(IReadOnlyList<BankMasterDto> Items, int Total)>;