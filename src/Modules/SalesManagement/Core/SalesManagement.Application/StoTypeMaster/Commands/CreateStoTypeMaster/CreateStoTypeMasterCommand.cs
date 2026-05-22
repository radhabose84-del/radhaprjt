using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.StoTypeMaster.Commands.CreateStoTypeMaster
{
    public class CreateStoTypeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? StoTypeCode { get; set; }
        public string? StoTypeName { get; set; }
        public string? Description { get; set; }
        public int PgiMovementTypeId { get; set; }
        public int GrMovementTypeId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
