using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.DepartmentGroup.Queries.GetDepartmentGroupAutoSearch
{
    public class GetDepartmentGroupAutoCompleteQuery   : IRequest<List<DepartmentGroupAutoCompleteDto>>
    {
          public string? SearchPattern { get; set; }
    }
}