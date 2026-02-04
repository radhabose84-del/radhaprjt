using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using MediatR;

namespace UserManagement.Application.PwdComplexityRule.Queries
{
    public class GetPwdRuleQuery : IRequest<ApiResponseDTO<List<GetPwdRuleDto>> >
    {
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }

    }
}