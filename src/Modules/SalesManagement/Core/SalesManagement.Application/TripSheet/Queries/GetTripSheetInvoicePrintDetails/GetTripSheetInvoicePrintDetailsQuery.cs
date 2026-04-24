using MediatR;
using SalesManagement.Application.Invoice.Dto;

namespace SalesManagement.Application.TripSheet.Queries.GetTripSheetInvoicePrintDetails
{
    public class GetTripSheetInvoicePrintDetailsQuery : IRequest<List<InvoicePrintDto>>
    {
        public int TripSheetHeaderId { get; set; }
    }
}
