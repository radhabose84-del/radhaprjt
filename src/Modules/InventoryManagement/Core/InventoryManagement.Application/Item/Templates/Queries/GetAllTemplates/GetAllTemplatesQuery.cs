// GetAll (PageNumber, PageSize, SearchTerm)
namespace InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplates
{
    using InventoryManagement.Application.Item.Templates.DTOs;
    using MediatR;
    public sealed record GetAllTemplatesQuery : IRequest<PagedResult<TemplateListItemDto>>
    {
        public string? SearchTerm { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
