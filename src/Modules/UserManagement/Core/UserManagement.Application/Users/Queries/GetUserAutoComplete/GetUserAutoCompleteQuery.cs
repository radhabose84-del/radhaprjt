using MediatR;
using UserManagement.Application.Users.Queries.GetUsers;
using UserManagement.Application.Common.HttpResponse;

namespace UserManagement.Application.Users.Queries.GetUserAutoComplete
{
    public class GetUserAutoCompleteQuery: IRequest<ApiResponseDTO<List<UserAutoCompleteDto>>>
    {
        public string? SearchPattern { get; set; }
    }
}