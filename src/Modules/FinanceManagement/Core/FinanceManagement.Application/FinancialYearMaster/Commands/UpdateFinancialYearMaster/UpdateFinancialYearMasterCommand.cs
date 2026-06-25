using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Commands.UpdateFinancialYearMaster
{
    public class UpdateFinancialYearMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? FinancialYearCode { get; set; }
        public int IsActive { get; set; }
        // StartDate / EndDate / StatusId are NOT user-editable in this story.
        // Year-end close (US-GL03-05) flips StatusId via a dedicated command.
        // Period status transitions belong to US-GL03-02.

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
