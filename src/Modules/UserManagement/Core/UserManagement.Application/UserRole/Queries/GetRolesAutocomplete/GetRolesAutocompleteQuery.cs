using MediatR;

namespace UserManagement.Application.UserRole.Queries.GetRolesAutocomplete
{
    public class GetRolesAutocompleteQuery : IRequest<List<GetUserRoleAutocompleteDto>>
    {
        public string? SearchTerm { get; set; }
    }
}