using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using MediatR;
using Contracts.Common;

public record UpdateImportPOCommand : IRequest<bool>, IRequirePermission
{
    public required ImportPOUpdateDto Data { get; init; }
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}
