using MediatR;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetReturnTypeById;

public sealed record GetReturnTypeByIdQuery(int Id) : IRequest<ReturnTypeDto?>;
