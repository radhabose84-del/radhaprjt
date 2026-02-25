using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermAutoComplete
{
    public class GetPaymentTermAutoCompleteQuery : IRequest<List<AutoCompleteDto>>
    {

        public string? SearchPattern { get; set; }
        public string? PaymentTermCode { get; set; }
             
    }
}