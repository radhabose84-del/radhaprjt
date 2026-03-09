using MediatR;

namespace UserManagement.Application.Divisions.Queries.GetUnitsByDivision
{
    public class GetUnitsByDivisionQuery : IRequest<List<GetUnitsByDivisionDto>>
    {
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
    }
}
