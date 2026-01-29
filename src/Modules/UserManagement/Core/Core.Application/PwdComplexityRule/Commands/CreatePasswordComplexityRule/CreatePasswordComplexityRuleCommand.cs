using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.PwdComplexityRule.Queries;
using MediatR;

namespace Core.Application.PwdComplexityRule.Commands.CreatePasswordComplexityRule
{
    public class CreatePasswordComplexityRuleCommand : IRequest<PwdRuleDto>
    {

    
       
        public string? PwdComplexityRule  { get; set; }

   
        
    }
}