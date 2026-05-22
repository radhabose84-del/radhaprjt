using MediatR;
using Contracts.Common;

namespace FAM.Application.UOM.Command.UpdateUOM
{
    public class UpdateUOMCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? UOMName { get; set; }
        public int SortOrder { get; set; }
        public int UOMTypeId { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
