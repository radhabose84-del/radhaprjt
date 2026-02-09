using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Queries.ProjectMasterAutoComplete
{
    public class GetProjectMasterAutoCompleteQuery : IRequest<List<ProjectMasterAutoCompleteDto>>
    {

        public int? UnitId { get; set; }
        public int? DepartmentId { get; set; }
        public string? SearchTerm { get; set; }
        public int Take { get; set; } = 10;
        public string? ProjectStatus { get; set; }
        

    }
}