using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Print.Dto;

namespace PurchaseManagement.Application.PurchaseOrder.Print.Queries.GetPurchaseOrderPrintDetails;

public sealed record GetPurchaseOrderPrintDetailsQuery(int Id) : IRequest<PurchaseOrderPrintDto?>;
