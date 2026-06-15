using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Commands.CreateAccountTypeMaster
{
    public class CreateAccountTypeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int CompanyId { get; set; }
        public string? AccountTypeName { get; set; }
        public string? StartCode { get; set; }
        public int AccountCodeLength { get; set; }
        public int SortOrder { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
