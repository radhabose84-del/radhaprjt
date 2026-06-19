using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Commands.ResetVoucherTypeSeries
{
    public class ResetVoucherTypeSeriesCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int VoucherTypeId { get; set; }
        public int FinancialYearId { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
