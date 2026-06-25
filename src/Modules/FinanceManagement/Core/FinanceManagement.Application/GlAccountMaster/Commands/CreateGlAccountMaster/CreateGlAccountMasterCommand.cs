using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.CreateGlAccountMaster
{
    public class CreateGlAccountMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int AccountTypeId { get; set; }
        public int AccountGroupId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? Description { get; set; }
        public int NormalBalanceId { get; set; }
        public int CurrencyTypeId { get; set; }
        public int SubLedgerTypeId { get; set; }
        public bool IsCostCentreMandatory { get; set; }
        public bool IsProfitCentreMandatory { get; set; }
        public bool IsTaxRelevant { get; set; }
        public bool IsInterCompany { get; set; }
        public bool IsReconciliationRequired { get; set; }

        // US-GL02-10 — when true the Group FC is authoring a global/template account (must be on the
        // template company); it fans out to every subsidiary of the entity (AC1/AC3).
        public bool IsGlobal { get; set; }

        // US-GL02-10 (AC2) — restricts the account to its owning entity: never inherited/propagated and
        // not postable from another entity. Mutually exclusive with IsGlobal.
        public bool IsCompanyRestricted { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
