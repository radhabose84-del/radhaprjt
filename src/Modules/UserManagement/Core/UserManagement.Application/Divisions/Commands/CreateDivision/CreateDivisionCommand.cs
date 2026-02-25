using UserManagement.Application.Divisions.Queries.GetDivisions;
using MediatR;

namespace UserManagement.Application.Divisions.Commands.CreateDivision
{
    public class CreateDivisionCommand : IRequest<DivisionDTO>
    {
        
        public string? ShortName { get; set; }
        public string? Name { get; set; }
        public int CompanyId { get; set; }
    }
}