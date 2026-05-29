using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Queries.GetById;

public sealed record GetBlanketPOByIdQuery(int Id) : IRequest<BlanketPODetailVm?>;
