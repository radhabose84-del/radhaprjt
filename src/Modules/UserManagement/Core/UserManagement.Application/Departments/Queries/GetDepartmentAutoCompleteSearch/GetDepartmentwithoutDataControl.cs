using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch
{

    public class GetDepartmentwithoutDataControl : IRequest<ApiResponseDTO<List<DepartmentAutocompleteDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}