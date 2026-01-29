using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter
{
    public class WorkCenterDto
    {
        public int Id { get; set; }
        public string? WorkCenterCode { get; set; }
        public string? WorkCenterName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Status IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}