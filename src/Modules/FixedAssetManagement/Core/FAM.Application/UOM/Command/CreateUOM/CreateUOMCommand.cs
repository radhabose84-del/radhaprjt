using FAM.Application.UOM.Queries.GetUOMs;
using MediatR;
using Contracts.Common;

namespace FAM.Application.UOM.Command.CreateUOM
{
    public class CreateUOMCommand : IRequest<UOMDto>, IRequirePermission
    {
        public string? Code { get; set; }
        public string? UOMName { get; set; }
        public int SortOrder { get; set; }
        public int UOMTypeId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
