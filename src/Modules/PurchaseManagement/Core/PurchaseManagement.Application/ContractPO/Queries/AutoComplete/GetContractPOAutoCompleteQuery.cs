using MediatR;
using PurchaseManagement.Application.ContractPO.Dto;

namespace PurchaseManagement.Application.ContractPO.Queries.AutoComplete;

public sealed record GetContractPOAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<ContractPOLookupDto>>;
