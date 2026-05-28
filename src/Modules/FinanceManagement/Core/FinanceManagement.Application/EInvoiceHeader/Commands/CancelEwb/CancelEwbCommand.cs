using Contracts.Common;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.CancelEwb
{
    public class CancelEwbCommand : IRequest<ApiResponseDTO<NicCancelEwbResultDto>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
        public int EInvoiceHeaderId { get; set; }

        /// <summary>
        /// Cancellation reason code (number):
        /// 1 = Duplicate, 2 = Data entry mistake, 3 = Order cancelled, 4 = Others
        /// </summary>
        public int CancelRsnCode { get; set; }

        /// <summary>
        /// Optional cancellation remarks
        /// </summary>
        public string? CancelRmrk { get; set; }
    }
}
