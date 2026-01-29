using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.PwdComplexityRule.Queries.GetPwdComplexityRule
{
    public class GetPwdRuleDto
    {


        public int Id { get; set; }

        public string? PwdComplexityRule { get; set; }

        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
        
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }

    }
}