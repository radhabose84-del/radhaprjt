using UserManagement.Application.State.Queries.GetStates;
using MediatR;

namespace UserManagement.Application.State.Queries.GetStateByCountryId
{
    public class GetStateByCountryIdQuery : IRequest<List<StateDto>>
    {
        public int Id { get; set; }
    }
}