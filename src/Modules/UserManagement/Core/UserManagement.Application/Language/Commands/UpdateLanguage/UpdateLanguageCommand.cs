using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Language.Commands.UpdateLanguage
{
    public class UpdateLanguageCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
