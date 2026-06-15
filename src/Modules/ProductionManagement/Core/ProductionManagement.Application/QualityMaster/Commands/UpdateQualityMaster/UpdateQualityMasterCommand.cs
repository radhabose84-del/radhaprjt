using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.QualityMaster.Commands.UpdateQualityMaster
{
    public class UpdateQualityMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? QualityName { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
