using MediatR;

namespace UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch
{

    public class GetDepartmentAutoCompleteSearchQuery : IRequest<List<DepartmentAutocompleteDto>>
    {
        public string? SearchPattern { get; set; } 
    }
}