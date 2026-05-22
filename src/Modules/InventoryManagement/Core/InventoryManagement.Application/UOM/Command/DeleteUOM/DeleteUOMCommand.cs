using Contracts.Common;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using MediatR;

namespace InventoryManagement.Application.UOM.Command.DeleteUOM
{
    public class DeleteUOMCommand  : IRequest<ApiResponseDTO<UOMDto>>, IRequirePermission
    {
        public int Id { get; set; }
        
        
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
