using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory
{
    public class MaintenanceCategoryAutoCompleteDto
    {
          public int Id { get; set; }
        public string? CategoryName { get; set; }
    }
}