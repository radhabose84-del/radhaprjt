using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ComplaintResolution.Commands.SubmitResolution
{
    public class SubmitResolutionCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
        public int ComplaintHeaderId { get; set; }
        public int ResolutionTypeId { get; set; }
        public string? ResolutionSummary { get; set; }

        // Sales Return
        public decimal? ReturnQuantity { get; set; }
        public int? ReturnLocationId { get; set; }
        public int? ReturnStatusId { get; set; }

        // Credit Note
        public decimal? CreditAmount { get; set; }
        public string? FinanceReference { get; set; }

        // Replacement
        public decimal? ReplacementQuantity { get; set; }
        public string? DispatchReference { get; set; }

        // Reprocess
        public string? ActionDescription { get; set; }

        // Closure
        public int? ClosureStatusId { get; set; }
        public string? ClosureRemarks { get; set; }
    }
}
