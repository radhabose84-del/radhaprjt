using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.FinancialYear.Queries.GetFinancialYearAutoComplete
{
    public class GetFinancialYearAutoCompleteDto
    {
         public int Id { get; set; }
         public string? StartYear { get; set; }

         public string? StartDate { get; set; } 
         
         public string? EndDate { get; set; } 
    }
}