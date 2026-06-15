using MediatR;
using Contracts.Common;

namespace UserManagement.Application.PwdComplexityRule.Commands.DeletePasswordComplexityRule
{
    public class DeletePasswordComplexityRuleCommand  :IRequest<int>, IRequirePermission
    {

          public int Id { get; set; }
       
         
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
