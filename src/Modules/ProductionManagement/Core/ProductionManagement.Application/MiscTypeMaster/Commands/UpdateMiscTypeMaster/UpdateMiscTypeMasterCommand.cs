using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster
{
    public class UpdateMiscTypeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
