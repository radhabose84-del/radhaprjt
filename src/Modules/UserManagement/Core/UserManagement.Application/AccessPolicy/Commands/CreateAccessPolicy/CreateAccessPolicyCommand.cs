using Contracts.Common;
using MediatR;

namespace UserManagement.Application.AccessPolicy.Commands.CreateAccessPolicy
{
    public class CreateAccessPolicyCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string PolicyCode { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string FieldName  { get; set; } = string.Empty;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
