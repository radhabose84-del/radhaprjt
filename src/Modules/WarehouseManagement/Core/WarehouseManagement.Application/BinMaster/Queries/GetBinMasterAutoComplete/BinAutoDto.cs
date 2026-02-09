using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManagement.Application.BinMaster.Queries.GetBinMasterAutoComplete
{
    public class BinAutoDto
    {
       public  int Id { get; set; }
       public string? BinCode { get; set; }
       public string? BinName { get; set; }
    }
}