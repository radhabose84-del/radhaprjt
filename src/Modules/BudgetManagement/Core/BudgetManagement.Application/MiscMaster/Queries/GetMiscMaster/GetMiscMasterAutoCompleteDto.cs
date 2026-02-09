using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetManagement.Application.MiscMaster.Queries.GetMiscMaster
{
    public class GetMiscMasterAutoCompleteDto
    {
         public int Id { get; set; }
         public string? Code { get; set;}
         public string? Description { get; set;}
    }
}