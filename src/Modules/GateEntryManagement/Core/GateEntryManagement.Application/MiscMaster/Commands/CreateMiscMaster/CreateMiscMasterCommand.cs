using Contracts.Common;
using MediatR;

namespace GateEntryManagement.Application.MiscMaster.Commands.CreateMiscMaster
{
    public class CreateMiscMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
