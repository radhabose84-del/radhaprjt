using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Commands.CreateCurrencyForexConfig
{
    public class CreateCurrencyForexConfigCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? CurrencyTypeCode { get; set; }
        public string? CurrencyTypeName { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
