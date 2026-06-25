using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Commands.CreateFinancialYearMaster
{
    public class CreateFinancialYearMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? FinancialYearCode { get; set; }     // "2024-25"
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsTransitionYear { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
