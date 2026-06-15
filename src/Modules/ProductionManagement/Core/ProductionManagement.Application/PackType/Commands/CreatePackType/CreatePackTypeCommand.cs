using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.PackType.Commands.CreatePackType
{
    public class CreatePackTypeCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? PackTypeCode { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeight { get; set; }
        public decimal TareWeight { get; set; }
        public int? ConesPerBag { get; set; }
        public int? PackMaterialId { get; set; }
        public bool ProductionAllowed { get; set; } = true;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
