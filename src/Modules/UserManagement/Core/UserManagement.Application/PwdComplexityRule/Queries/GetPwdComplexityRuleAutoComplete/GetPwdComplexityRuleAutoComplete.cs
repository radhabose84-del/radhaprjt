using MediatR;

namespace UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleAutoComplete
{
    public class GetPwdComplexityRuleAutoComplete : IRequest<List<PwdComplexityRuleAutoCompleteDto>>
    {
                public string? SearchTerm  { get; set; } 
    }
}