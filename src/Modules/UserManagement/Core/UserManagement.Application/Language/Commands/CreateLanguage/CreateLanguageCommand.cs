using UserManagement.Application.Language.Queries.GetLanguages;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Language.Commands.CreateLanguage
{
    public class CreateLanguageCommand : IRequest<LanguageDTO>, IRequirePermission
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
