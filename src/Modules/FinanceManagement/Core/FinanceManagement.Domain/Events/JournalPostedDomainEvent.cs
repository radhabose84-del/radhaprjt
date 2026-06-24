using MediatR;

namespace FinanceManagement.Domain.Events
{
    // Raised after a journal is successfully posted (US-GL01-09). Consumed by the flagging engine
    // (US-GL01-16B) — alert-only, must never block or roll back the posting.
    public class JournalPostedDomainEvent : INotification
    {
        public int JournalId { get; }
        public int CompanyId { get; }
        public decimal Amount { get; }          // posted total (Dr = Cr)
        public DateOnly? PostingDate { get; }
        public bool IsReversal { get; }

        public JournalPostedDomainEvent(int journalId, int companyId, decimal amount, DateOnly? postingDate, bool isReversal)
        {
            JournalId = journalId;
            CompanyId = companyId;
            Amount = amount;
            PostingDate = postingDate;
            IsReversal = isReversal;
        }
    }
}
