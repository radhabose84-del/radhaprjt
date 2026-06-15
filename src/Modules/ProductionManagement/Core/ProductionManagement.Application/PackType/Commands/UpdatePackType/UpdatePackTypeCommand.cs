using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.PackType.Commands.UpdatePackType
{
    public class UpdatePackTypeCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeight { get; set; }
        public decimal TareWeight { get; set; }
        public int? ConesPerBag { get; set; }
        public int? PackMaterialId { get; set; }
        public bool ProductionAllowed { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
