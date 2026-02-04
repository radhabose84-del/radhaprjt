using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupAutoSearch
{
    public class GetDepartmentGroupAutoCompleteQuery   : IRequest<List<DepartmentGroupAutoCompleteDto>>
    {
          public string? SearchPattern { get; set; }
    }
}