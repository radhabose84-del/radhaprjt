using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.InitializeSubsidiaryCoa
{
    // US-GL02-10 (AC1) — seed a subsidiary's COA from the global template. Idempotent: only the
    // template accounts the company does not already have (by link or code) are copied in.
    public class InitializeSubsidiaryCoaCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int CompanyId { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
