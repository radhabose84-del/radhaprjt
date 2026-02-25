using MediatR;

namespace UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule
{
    public class UpdatePasswordComplexityRuleCommand : IRequest<bool>
    {
        
        public int Id { get; set; }
       
        public string? PwdComplexityRule  { get; set; }

        public byte IsActive { get; set; }


    }
}