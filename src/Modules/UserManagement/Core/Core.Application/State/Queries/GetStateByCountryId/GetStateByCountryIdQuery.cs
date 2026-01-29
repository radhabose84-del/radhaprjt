using Core.Application.Common.HttpResponse;
using Core.Application.State.Queries.GetStates;
using MediatR;

namespace Core.Application.State.Queries.GetStateByCountryId
{
    public class GetStateByCountryIdQuery : IRequest<List<StateDto>>
    {
        public int Id { get; set; }
    }
}