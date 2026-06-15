using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Commands.UpdateAccountTypeMaster
{
    public class UpdateAccountTypeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? AccountTypeName { get; set; }
        public string? StartCode { get; set; }
        public int AccountCodeLength { get; set; }
        public int SortOrder { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
