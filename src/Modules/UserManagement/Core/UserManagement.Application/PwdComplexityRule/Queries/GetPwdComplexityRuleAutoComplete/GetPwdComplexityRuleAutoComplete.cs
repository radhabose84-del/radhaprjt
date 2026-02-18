using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleAutoComplete
{
    public class GetPwdComplexityRuleAutoComplete : IRequest<List<PwdComplexityRuleAutoCompleteDto>>
    {
                public string? SearchTerm  { get; set; } 
    }
}