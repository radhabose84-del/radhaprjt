namespace UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation
{
    public class CreateUserRoleAllocationDto
    {
            public int UserId { get; set; }
            public List<int> RoleIds { get; set; } = default!;
    }
}