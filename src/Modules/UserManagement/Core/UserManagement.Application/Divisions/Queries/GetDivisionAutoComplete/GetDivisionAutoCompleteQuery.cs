using UserManagement.Application.Divisions.Queries.GetDivisions;
using MediatR;

namespace UserManagement.Application.Divisions.Queries.GetDivisionAutoComplete
{
    public class GetDivisionAutoCompleteQuery : IRequest<List<DivisionAutoCompleteDTO>>
    {
        
        public string? SearchPattern { get; set; }
        public string? Companies { get; set; }
    }
}