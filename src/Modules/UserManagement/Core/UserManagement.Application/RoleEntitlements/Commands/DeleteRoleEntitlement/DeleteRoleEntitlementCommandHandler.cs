using Contracts.Common;
using MediatR;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements;

namespace UserManagement.Application.RoleEntitlements.Commands.DeleteRoleEntitlement
{
    public class DeleteRoleEntitlementCommandHandler : IRequestHandler<DeleteRoleEntitlementCommand, ApiResponseDTO<RoleEntitlementDto>>
    {
        public Task<ApiResponseDTO<RoleEntitlementDto>> Handle(DeleteRoleEntitlementCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ApiResponseDTO<RoleEntitlementDto>
            {
                IsSuccess = false,
                Message = "RoleEntitlement deletion is not supported."
            });
        }
    }
}
