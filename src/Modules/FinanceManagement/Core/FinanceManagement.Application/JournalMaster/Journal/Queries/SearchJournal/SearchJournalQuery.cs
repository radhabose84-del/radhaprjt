using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.SearchJournal
{
    // US-GL01 Journal List & Search — paged, multi-criteria search. All filters optional (AND-combined).
    public class SearchJournalQuery : IRequest<ApiResponseDTO<List<JournalListItemDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        public string? VoucherNo { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public int? AccountId { get; set; }
        public int? CostCentreId { get; set; }
        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }
        public int? VoucherTypeId { get; set; }
        public int? StatusId { get; set; }
        public int? CreatedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public int? SourceId { get; set; }
        public string? Narration { get; set; }
        public string? Reference { get; set; }
    }
}
