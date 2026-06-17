using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Commands.UpdateCurrencyForexConfig
{
    public class UpdateCurrencyForexConfigCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        // CurrencyTypeCode is immutable — not included
        public string? CurrencyTypeName { get; set; }
        public int IsActive { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
