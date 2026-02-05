// AutoComplete
namespace InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplateAutoComplete
{
    using InventoryManagement.Application.Item.Templates.DTOs;
    using MediatR;
    public sealed record GetTemplateAutoCompleteQuery : IRequest<List<TemplateAutoCompleteDto>>
    {
        public string? SearchPattern { get; init; }
        public int Take { get; init; } = 10;
    }
}
