using Contracts.Common;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Commands.GenerateStandaloneEwb
{
    // Orchestrator command for standalone (no-IRN) e-Waybill generation.
    // Called by SalesManagement DC handler after the EWaybillHeader row is inserted.
    // Calls NIC standalone EWB API, then updates the row with the EWB number on success
    // or with error details on failure (status stays Pending so operator can retry).
    public class GenerateStandaloneEwbCommand : IRequest<ApiResponseDTO<NicEwbResultDto>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanAdd;
        public int EWaybillHeaderId { get; set; }
        public StandaloneEwbPayloadDto Payload { get; set; } = new();
    }
}
