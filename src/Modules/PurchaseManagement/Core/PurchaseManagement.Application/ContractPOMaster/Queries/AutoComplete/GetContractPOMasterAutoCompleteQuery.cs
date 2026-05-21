using MediatR;
using PurchaseManagement.Application.ContractPOMaster.Dto;

namespace PurchaseManagement.Application.ContractPOMaster.Queries.AutoComplete;

public sealed record GetContractPOMasterAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<ContractPOLookupDto>>;
