using MediatR;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoCompleteComparison
{
    public class GetRfqAutoCompleteComparisonQuery : IRequest<List<RfqAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
        public DateOnly? LastSubmitDate { get; set; }
        public int? StatusId { get; set; }
    }
}
