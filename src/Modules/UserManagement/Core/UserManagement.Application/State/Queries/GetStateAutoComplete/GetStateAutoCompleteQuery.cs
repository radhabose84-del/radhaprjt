using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.State.Queries.GetStates;
using MediatR;

namespace UserManagement.Application.State.Queries.GetStateAutoComplete
{
    public class GetStateAutoCompleteQuery : IRequest<List<StateAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}
