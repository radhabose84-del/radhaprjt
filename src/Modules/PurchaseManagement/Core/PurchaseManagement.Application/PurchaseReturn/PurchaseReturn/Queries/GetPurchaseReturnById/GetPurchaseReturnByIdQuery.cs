using MediatR;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnById;

public sealed record GetPurchaseReturnByIdQuery(int Id) : IRequest<PurchaseReturnHeaderDto?>;
