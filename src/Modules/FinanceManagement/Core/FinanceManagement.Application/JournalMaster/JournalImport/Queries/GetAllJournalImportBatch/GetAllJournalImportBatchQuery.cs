using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetAllJournalImportBatch
{
    public class GetAllJournalImportBatchQuery : IRequest<ApiResponseDTO<List<JournalImportBatchDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
