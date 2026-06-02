using MediatR;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnableGrnsByVendorPo;

public sealed record GetReturnableGrnsByVendorPoQuery(int VendorId, int PoId)
    : IRequest<IReadOnlyList<PurchaseReturnGrnLookupDto>>;
