using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.MRS.Command.UpdateMrsEntry
{
    public class UpdateMrsEntryCommand  : IRequest<bool>, IRequirePermission
    {
        public UpdateMrsEntryDto updateMrsEntry { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
