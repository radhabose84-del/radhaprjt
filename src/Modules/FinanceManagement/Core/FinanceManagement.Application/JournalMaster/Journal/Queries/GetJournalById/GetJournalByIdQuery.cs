using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalById
{
    public class GetJournalByIdQuery : IRequest<JournalHeaderDto?>
    {
        public int Id { get; set; }
    }
}
