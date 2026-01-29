using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Reports.WorkOderCheckListReport
{
    public class WorkOderCheckListReportDto
    {
                       // WorkOrder Id
        public int CompanyId { get; set; }
        public string? CompanyName { get; set;}
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? WorkOrderDocNo { get; set; }
        public string? CreatedByName { get; set; }  
        public int MachineId { get; set; }      
        public string? MachineCode { get; set; }
        public string? MachineName { get; set; }
        public int MachineGroupID { get; set; }
        public string? GroupName { get; set; }   
        public int ActivityId { get; set; }    
        public string? ActivityName { get; set; }      
        public string? ActivityCheckList { get; set; }
        public bool ISCompleted { get; set; }
        public string? Remarks { get; set; }

    }
}