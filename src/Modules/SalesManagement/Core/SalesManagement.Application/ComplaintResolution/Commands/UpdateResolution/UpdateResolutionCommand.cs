using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ComplaintResolution.Commands.UpdateResolution
{
    public class UpdateResolutionCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
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

        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
