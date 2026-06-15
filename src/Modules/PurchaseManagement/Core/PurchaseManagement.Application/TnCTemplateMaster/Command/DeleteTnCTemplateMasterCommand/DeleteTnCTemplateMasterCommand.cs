using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.DeleteTnCTemplateMasterCommand
{
    public class DeleteTnCTemplateMasterCommand   : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
