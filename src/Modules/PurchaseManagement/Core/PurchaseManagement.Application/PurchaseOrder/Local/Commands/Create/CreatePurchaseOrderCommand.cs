
using PurchaseManagement.Application.Common.HttpResponse;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using MediatR;
using PurchaseLocalDetailDto.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create;

public record CreatePurchaseOrderCommand : IRequest<ApiResponseDTO<int>>
{
    public required PurchaseOrderCreateDto Data { get; init; } 
}