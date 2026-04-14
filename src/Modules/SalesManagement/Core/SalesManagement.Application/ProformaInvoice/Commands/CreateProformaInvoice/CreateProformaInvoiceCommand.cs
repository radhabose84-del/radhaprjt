using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ProformaInvoice.Commands.CreateProformaInvoice
{
    public class CreateProformaInvoiceCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateOnly ProformaDate { get; set; }
        public int SalesOrderId { get; set; }
        public int PartyId { get; set; }
        public decimal ProformaAmount { get; set; }
        public string? Remarks { get; set; }
        public int? StatusId { get; set; }
    }
}
