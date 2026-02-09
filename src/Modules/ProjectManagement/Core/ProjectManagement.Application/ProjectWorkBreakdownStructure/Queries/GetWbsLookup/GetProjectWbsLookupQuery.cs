using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWbsLookup
{
    public class GetProjectWbsLookupQuery : IRequest<List<ProjectWbsLookupDto>>
    {
         public int? ProjectId { get; set; }
    }
}