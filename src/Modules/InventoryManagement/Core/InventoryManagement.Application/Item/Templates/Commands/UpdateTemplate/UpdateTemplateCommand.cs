// Update
namespace InventoryManagement.Application.Item.Templates.Commands.UpdateTemplate
{
    using InventoryManagement.Application.Item.Templates.DTOs;
    using MediatR;
    public sealed record UpdateTemplateCommand(
        int Id,
        string TemplateName,
        List<TemplateParameterDto>? Parameters,
        int? IsActive
    ) : IRequest<bool>;
}
