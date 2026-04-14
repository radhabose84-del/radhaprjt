using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaPayment
{
    public class UpdateProformaPaymentCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public decimal PaymentReceivedAmount { get; set; }
    }
}
