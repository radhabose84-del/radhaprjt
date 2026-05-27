using MediatR;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonsByReturnType;

public sealed record GetReturnReasonsByReturnTypeQuery(int ReturnTypeId)
    : IRequest<IReadOnlyList<ReturnReasonLookupDto>>;
