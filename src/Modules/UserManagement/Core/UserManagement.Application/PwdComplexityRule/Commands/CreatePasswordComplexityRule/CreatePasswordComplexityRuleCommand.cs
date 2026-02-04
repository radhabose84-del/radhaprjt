using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.PwdComplexityRule.Queries;
using MediatR;

namespace UserManagement.Application.PwdComplexityRule.Commands.CreatePasswordComplexityRule
{
    public class CreatePasswordComplexityRuleCommand : IRequest<PwdRuleDto>
    {

    
       
        public string? PwdComplexityRule  { get; set; }

   
        
    }
}