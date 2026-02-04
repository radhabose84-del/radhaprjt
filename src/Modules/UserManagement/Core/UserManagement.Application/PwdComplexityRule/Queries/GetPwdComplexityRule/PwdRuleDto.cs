using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.PwdComplexityRule.Queries
{
    public class PwdRuleDto
    {
        public int Id { get; set; }
       
        public string ? PwdComplexityRule  { get; set; } 

       

    }
}