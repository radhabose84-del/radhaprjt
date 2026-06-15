using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
