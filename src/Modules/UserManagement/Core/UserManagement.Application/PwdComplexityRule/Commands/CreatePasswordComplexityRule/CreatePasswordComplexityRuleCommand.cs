using UserManagement.Application.PwdComplexityRule.Queries;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.PwdComplexityRule.Commands.CreatePasswordComplexityRule
{
    public class CreatePasswordComplexityRuleCommand : IRequest<PwdRuleDto>, IRequirePermission
    {

    
       
        public string? PwdComplexityRule  { get; set; }

   
        
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
