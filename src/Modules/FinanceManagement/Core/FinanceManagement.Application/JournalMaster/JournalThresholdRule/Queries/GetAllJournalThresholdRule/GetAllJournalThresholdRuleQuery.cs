using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetAllJournalThresholdRule
{
    public class GetAllJournalThresholdRuleQuery : IRequest<ApiResponseDTO<List<JournalThresholdRuleDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
