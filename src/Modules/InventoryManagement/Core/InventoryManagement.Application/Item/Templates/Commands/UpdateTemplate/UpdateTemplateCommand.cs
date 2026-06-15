// Update
namespace InventoryManagement.Application.Item.Templates.Commands.UpdateTemplate
{
    using InventoryManagement.Application.Item.Templates.DTOs;
    using Contracts.Common;
using MediatR;
    public sealed record UpdateTemplateCommand(
        int Id,
        string TemplateName,
        List<TemplateParameterDto>? Parameters,
        int? IsActive
    ) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}
}
