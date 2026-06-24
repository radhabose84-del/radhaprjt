using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetJournalImportBatchById
{
    public class GetJournalImportBatchByIdQuery : IRequest<JournalImportBatchDto?>
    {
        public int Id { get; set; }
    }
}
