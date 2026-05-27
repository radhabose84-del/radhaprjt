using MediatR;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnableQtyByGrn;

public sealed record GetReturnableQtyByGrnQuery(int GrnHeaderId)
    : IRequest<IReadOnlyList<ReturnableQtyDto>>;
