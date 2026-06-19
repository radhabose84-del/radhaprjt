using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Commands.UpdateVoucherType
{
    public class UpdateVoucherTypeCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        // VoucherTypeCode is immutable — not included
        public string? VoucherTypeName { get; set; }
        public int NumberPadding { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive

        public List<int> AllowedAccountTypeIds { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
