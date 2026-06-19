using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Commands.CreateVoucherType
{
    public class CreateVoucherTypeCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        // CompanyId is taken from the session token (IIPAddressService) — never from the payload.
        public string? VoucherTypeCode { get; set; }   // also the series prefix — immutable after create
        public string? VoucherTypeName { get; set; }
        public int NumberPadding { get; set; }

        // No fiscal year on create — the series operates on the current FY (resolved at read time);
        // counter rows are created lazily by reset-series or the numbering engine (FR-003).

        // Allowed account types (ids from Finance.AccountTypeMaster)
        public List<int> AllowedAccountTypeIds { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
