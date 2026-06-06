using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Commands.SubmitFreightRfqForApproval
{
    /// <summary>Selects the preferred transporter and submits the RFQ for MD approval.</summary>
    public class SubmitFreightRfqForApprovalCommand : IRequest<ApiResponseDTO<int>>
    {
        public int FreightRfqId { get; set; }
        public int SelectedQuotationId { get; set; }
        public bool IsOverride { get; set; }            // 1 when the selected row is not the lowest quote
        public string? ComparisonRemarks { get; set; }   // mandatory when IsOverride (validator)
    }
}
