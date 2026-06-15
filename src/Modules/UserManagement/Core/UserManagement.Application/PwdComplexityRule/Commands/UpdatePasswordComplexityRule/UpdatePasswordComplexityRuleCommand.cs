using MediatR;
using Contracts.Common;

namespace UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule
{
    public class UpdatePasswordComplexityRuleCommand : IRequest<bool>, IRequirePermission
    {
        
        public int Id { get; set; }
       
        public string? PwdComplexityRule  { get; set; }

        public byte IsActive { get; set; }


        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
