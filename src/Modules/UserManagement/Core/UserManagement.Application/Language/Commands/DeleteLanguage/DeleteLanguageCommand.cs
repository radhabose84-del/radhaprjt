using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Language.Commands.DeleteLanguage
{
    public class DeleteLanguageCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
