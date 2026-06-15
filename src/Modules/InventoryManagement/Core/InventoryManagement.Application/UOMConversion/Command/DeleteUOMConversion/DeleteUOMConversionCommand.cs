using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion
{
    public class DeleteUOMConversionCommand : IRequest<bool>, IRequirePermission
    {
          public int Id { get; set; }
        
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
