using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentAutoComplete
{
    public class GetPurchaseIndentAutoCompleteQuery : IRequest<List<PurchaseIndentAutoCompleteQueryDto>>
    {
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public Boolean AllIndents { get; set; }=false;
    }
}