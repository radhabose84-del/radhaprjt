using MediatR;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoComplete
{
    public class GetRfqAutoCompleteQuotationQuery : IRequest<List<RfqAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
        public DateOnly? LastSubmitDate { get; set; }
    }
}
