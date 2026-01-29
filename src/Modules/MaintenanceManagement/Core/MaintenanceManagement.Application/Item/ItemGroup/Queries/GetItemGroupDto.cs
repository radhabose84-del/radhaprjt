using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Item.ItemGroup.Queries
{
    public class GetItemGroupDto
    {
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
    }
}