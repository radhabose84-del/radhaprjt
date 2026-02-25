using MediatR;

namespace UserManagement.Application.PwdComplexityRule.Commands.DeletePasswordComplexityRule
{
    public class DeletePasswordComplexityRuleCommand  :IRequest<int>
    {

          public int Id { get; set; }
       
         
    }
}