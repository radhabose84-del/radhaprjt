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
        public bool IsTaxRelevant { get; set; }
        public bool IsInterCompany { get; set; }
        public bool IsReconciliationRequired { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
