using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Project
{
    public class ProjectMasterAutoCompleteDto
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; } = default!;
        public string ProjectName { get; set; } = default!;
        public string? ProjectDescription { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public int StatusId { get; set; }
    }
}