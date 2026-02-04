namespace InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate
{
    using MediatR;
    public sealed record DeleteTemplateCommand : IRequest<bool>
    {
        public int Id { get; init; }
    }
}
