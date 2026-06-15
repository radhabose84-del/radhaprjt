using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.RawMaterialType.Commands.UpdateRawMaterialType
{
    public class UpdateRawMaterialTypeCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string RawMaterialTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
