using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Mappings;
using Core.Domain.Entities;
using Core.Domain.Enums;


namespace Core.Application.FinancialYear.Queries.GetFinancialYear
{
    public class FinancialYearDto  : IMapFrom<ApiResponseDTO<Core.Domain.Entities.FinancialYear>>
    {
         public int Id { get; set; }
        public string? StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 

        public string? FinYearName { get; set; }


        


    }
}