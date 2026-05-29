using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;

namespace PurchaseManagement.Application.BlanketMaster.Queries.AutoComplete;

public sealed record GetBlanketMasterAutoCompleteQuery(
    string Term,
    bool ApprovedOnly = true,
    int? VendorId = null,
    DateTimeOffset? PODate = null
) : IRequest<IReadOnlyList<BlanketMasterLookupDto>>;
