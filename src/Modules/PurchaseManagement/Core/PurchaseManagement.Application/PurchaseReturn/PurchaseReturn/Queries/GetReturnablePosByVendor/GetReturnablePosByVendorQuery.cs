using MediatR;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnablePosByVendor;

public sealed record GetReturnablePosByVendorQuery(int VendorId)
    : IRequest<IReadOnlyList<PurchaseReturnPoLookupDto>>;
