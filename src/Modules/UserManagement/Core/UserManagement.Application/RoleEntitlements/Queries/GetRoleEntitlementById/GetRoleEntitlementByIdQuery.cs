using MediatR;

namespace UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlementById
{
    public class GetRoleEntitlementByIdQuery: IRequest<GetByIdRoleEntitlementDTO>
    {
        public int Id { get; set; }
        
        
    }
}