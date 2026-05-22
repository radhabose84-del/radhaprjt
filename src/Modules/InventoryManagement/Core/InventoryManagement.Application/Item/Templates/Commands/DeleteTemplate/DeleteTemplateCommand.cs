namespace InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate
{
    using MediatR;
using Contracts.Common;
    public sealed record DeleteTemplateCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; init; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
