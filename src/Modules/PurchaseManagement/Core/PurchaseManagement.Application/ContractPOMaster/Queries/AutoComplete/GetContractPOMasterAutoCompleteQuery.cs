using MediatR;
using PurchaseManagement.Application.ContractPOMaster.Dto;

namespace PurchaseManagement.Application.ContractPOMaster.Queries.AutoComplete;

public sealed record GetContractPOMasterAutoCompleteQuery(string Term, bool ApprovedOnly = true, int? VendorId = null)
    : IRequest<IReadOnlyList<ContractPOLookupDto>>;
