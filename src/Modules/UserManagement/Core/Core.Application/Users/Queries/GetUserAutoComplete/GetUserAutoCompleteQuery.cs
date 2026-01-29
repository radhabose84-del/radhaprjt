using MediatR;
using Core.Application.Users.Queries.GetUsers;
using Core.Application.Common.HttpResponse;

namespace Core.Application.Users.Queries.GetUserAutoComplete
{
    public class GetUserAutoCompleteQuery: IRequest<ApiResponseDTO<List<UserAutoCompleteDto>>>
    {
        public string? SearchPattern { get; set; }
    }
}