// PurchaseManagement.Application/PriceMaster/Commands/Delete/SoftDeletePriceMasterCommand.cs
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.PriceMaster.Commands.Delete
{
    public sealed class DeletePriceMasterCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; init; }        
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
