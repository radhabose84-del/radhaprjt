using Contracts.Common;
using MediatR;

namespace UserManagement.Application.AccessPolicy.Commands.UpdateAccessPolicy
{
    public class UpdateAccessPolicyCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int    Id         { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string FieldName  { get; set; } = string.Empty;
        public int    IsActive   { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
