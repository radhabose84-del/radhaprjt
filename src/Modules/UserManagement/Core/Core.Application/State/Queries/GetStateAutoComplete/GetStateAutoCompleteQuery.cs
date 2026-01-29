using Core.Application.Common.HttpResponse;
using Core.Application.State.Queries.GetStates;
using MediatR;

namespace Core.Application.State.Queries.GetStateAutoComplete
{
    public class GetStateAutoCompleteQuery : IRequest<List<StateAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}
