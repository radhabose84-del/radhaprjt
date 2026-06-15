using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster
{
    public class UpdateTransactionTypeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public int MenuId { get; set; }
        public string? TypeName { get; set; }
        public string? ShortName { get; set; }
        public string? Description { get; set; }
        public int IsGate { get; set; }     // 0 = No, 1 = Yes
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
