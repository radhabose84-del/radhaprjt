using UserManagement.Application.State.Queries.GetStates;
using MediatR;

namespace UserManagement.Application.State.Queries.GetStateById
{
    public class GetStateByIdQuery : IRequest<StateDto>
    {
        public int Id { get; set; }
    }
}