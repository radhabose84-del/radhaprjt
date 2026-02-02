using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using MediatR;

public record UpdateImportPOCommand : IRequest<bool>
{
    public required ImportPOUpdateDto Data { get; init; }
}
