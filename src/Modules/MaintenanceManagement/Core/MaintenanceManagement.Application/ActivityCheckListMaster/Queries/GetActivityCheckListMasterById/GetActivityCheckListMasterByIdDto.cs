using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMasterById
{
    public class GetActivityCheckListMasterByIdDto
    {
        
      public int ChecklistId { get; set; }
      public int ActivityID { get; set; }      
      public string? ActivityChecklist { get; set; }
      public int UnitId { get; set; }
      public Status IsActive { get; set; } 
    }
}