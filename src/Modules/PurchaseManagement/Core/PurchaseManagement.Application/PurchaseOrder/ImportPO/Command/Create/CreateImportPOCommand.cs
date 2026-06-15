using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using MediatR;
using Contracts.Common;
namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
public sealed record CreateImportPOCommand : IRequest<int>, IRequirePermission
{
    public ImportPOCreateDto Data { get; init; }= default!;
    public PermissionType RequiredPermission => PermissionType.CanAdd;
}
