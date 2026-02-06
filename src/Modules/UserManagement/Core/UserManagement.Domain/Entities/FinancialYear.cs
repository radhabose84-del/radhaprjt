using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Common;
using static UserManagement.Domain.Enums.FinancialYearEnum;

namespace UserManagement.Domain.Entities
{
    public class  FinancialYear : BaseEntity
    {   
        public int Id { get; set; }
        public string? StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 

        public string? FinYearName { get; set; }
         public CompanySettings? CompanySettings { get; set; }
    }
}