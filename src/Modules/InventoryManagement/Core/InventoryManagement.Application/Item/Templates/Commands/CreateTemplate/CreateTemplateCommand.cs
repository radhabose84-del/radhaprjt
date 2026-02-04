// Create
namespace InventoryManagement.Application.Item.Templates.Commands.CreateTemplate
{
    using InventoryManagement.Application.Item.Templates.DTOs;
    using MediatR;
    public sealed record CreateTemplateCommand(
        string TemplateName,
        List<TemplateParameterDto>? Parameters
    ) : IRequest<int>;
}
