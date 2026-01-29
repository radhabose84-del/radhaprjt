using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Users
{
    public class FinancialYearDto
    {
        public int Id { get; set; }
        public string StartYear { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public string FinYearName { get; set; }
        
    }
}