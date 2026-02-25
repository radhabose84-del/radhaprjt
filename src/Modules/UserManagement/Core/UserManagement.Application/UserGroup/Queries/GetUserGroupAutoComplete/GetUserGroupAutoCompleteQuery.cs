using MediatR;

namespace UserManagement.Application.UserGroup.Queries.GetUserGroupAutoComplete
{
    public class GetUserGroupAutoCompleteQuery : IRequest<List<UserGroupAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}