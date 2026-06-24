using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetJournalThresholdRuleById
{
    public class GetJournalThresholdRuleByIdQuery : IRequest<JournalThresholdRuleDto?>
    {
        public int Id { get; set; }
    }
}
