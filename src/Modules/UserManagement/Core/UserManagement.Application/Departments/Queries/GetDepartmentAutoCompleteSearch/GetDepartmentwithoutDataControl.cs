using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;

namespace UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch
{

    public class GetDepartmentwithoutDataControl : IRequest<ApiResponseDTO<List<DepartmentAutocompleteDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}