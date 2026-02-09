using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManagement.Application.RackMaster.Queries.GetRackMasterAutoComplete
{
    public class GetRackMasterAutoCompleteDto
    {

        public int Id { get; set; }
        public string? RackCode { get; set; }
        public string? RackName { get; set; }
        
        
    }
}