using MediatR;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonById;

public sealed record GetReturnReasonByIdQuery(int Id) : IRequest<ReturnReasonDto?>;
