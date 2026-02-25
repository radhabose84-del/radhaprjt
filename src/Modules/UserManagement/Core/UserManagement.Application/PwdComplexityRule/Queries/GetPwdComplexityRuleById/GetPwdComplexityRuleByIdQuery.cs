using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using MediatR;

namespace UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleById
{
    public class GetPwdComplexityRuleByIdQuery :IRequest<GetPwdRuleDto>
    {
         public int Id { get; set; }
    }
}