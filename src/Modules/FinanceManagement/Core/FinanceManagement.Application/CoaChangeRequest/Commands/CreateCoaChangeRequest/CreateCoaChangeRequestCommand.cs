using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaChangeRequest
{
    // US-GL02-08B — raise a change request against a sealed COA. CompanyId / requester come from session.
    // The ImpactAssessment is mandatory (AC5) and must be CFO-approved before any unfreeze approval proceeds.
    public class CreateCoaChangeRequestCommand : IRequest<ApiResponseDTO<int>>
    {
        public int? TargetAccountId { get; set; }
        public int? TargetAccountGroupId { get; set; }
        public string? AccountCodeSnapshot { get; set; }
        public string ChangeType { get; set; } = null!;
        public string Justification { get; set; } = null!;
        public string ImpactAssessment { get; set; } = null!;
    }
}
