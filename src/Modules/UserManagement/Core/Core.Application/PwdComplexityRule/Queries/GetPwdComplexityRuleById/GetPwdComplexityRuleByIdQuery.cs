using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using MediatR;

namespace Core.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleById
{
    public class GetPwdComplexityRuleByIdQuery :IRequest<GetPwdRuleDto>
    {
         public int Id { get; set; }
    }
}