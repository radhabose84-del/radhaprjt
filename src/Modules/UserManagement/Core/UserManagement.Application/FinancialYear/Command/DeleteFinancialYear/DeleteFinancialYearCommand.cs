using MediatR;
using Contracts.Common;

namespace UserManagement.Application.FinancialYear.Command.DeleteFinancialYear
{
    public class DeleteFinancialYearCommand :IRequest<int>, IRequirePermission
    {

          public int Id { get; set; }
        
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
