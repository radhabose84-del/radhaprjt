
using Contracts.Common;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create;

public record CreatePurchaseOrderCommand : IRequest<ApiResponseDTO<int>>
{
    public required PurchaseOrderCreateDto Data { get; init; } 
}