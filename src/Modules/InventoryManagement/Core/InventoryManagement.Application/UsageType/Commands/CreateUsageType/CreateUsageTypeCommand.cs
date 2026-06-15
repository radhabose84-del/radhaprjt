using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.UsageType.Commands.CreateUsageType
{
    public class CreateUsageTypeCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? UsageTypeCode { get; set; }
        public string? UsageTypeName { get; set; }
        public string? Description { get; set; }
        public int ModuleId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
