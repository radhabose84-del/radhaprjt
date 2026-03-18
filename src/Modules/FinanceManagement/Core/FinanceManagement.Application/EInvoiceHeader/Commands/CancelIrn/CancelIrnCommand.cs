using Contracts.Common;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.CancelIrn
{
    public class CancelIrnCommand : IRequest<ApiResponseDTO<NicCancelIrnResultDto>>
    {
        public int EInvoiceHeaderId { get; set; }

        /// <summary>
        /// Cancellation reason code:
        /// "1" = Duplicate, "2" = Data entry mistake, "3" = Order cancelled, "4" = Others
        /// </summary>
        public string? CnlRsn { get; set; }

        /// <summary>
        /// Optional cancellation remarks (max 100 characters)
        /// </summary>
        public string? CnlRem { get; set; }
    }
}
