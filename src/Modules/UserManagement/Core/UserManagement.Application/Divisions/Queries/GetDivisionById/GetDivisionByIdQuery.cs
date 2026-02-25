using MediatR;
using UserManagement.Application.Divisions.Queries.GetDivisions;

namespace UserManagement.Application.Divisions.Queries.GetDivisionById
{
    public class GetDivisionByIdQuery : IRequest<DivisionDTO>
    {
        public int Id { get; set; }
    }
}