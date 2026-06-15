// Create
namespace InventoryManagement.Application.Item.Templates.Commands.CreateTemplate
{
    using InventoryManagement.Application.Item.Templates.DTOs;
    using Contracts.Common;
using MediatR;
    public sealed record CreateTemplateCommand(
        string TemplateName,
        List<TemplateParameterDto>? Parameters
    ) : IRequest<int>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanAdd;
}
}
