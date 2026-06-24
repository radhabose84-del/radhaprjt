using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringJournalTemplateById
{
    public class GetRecurringJournalTemplateByIdQuery : IRequest<RecurringJournalTemplateHeaderDto?>
    {
        public int Id { get; set; }
    }
}
