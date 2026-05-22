using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.MRS.Command.CreateMrsEntry
{
    public class CreateMrsEntryCommand : IRequest<int>, IRequirePermission
    {
        public CreateMrsEntryDto MrsEntry { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
