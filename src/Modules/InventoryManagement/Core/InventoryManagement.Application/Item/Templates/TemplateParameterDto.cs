namespace InventoryManagement.Application.Item.Templates.DTOs
{
    public sealed class TemplateParameterDto
    {
        public int? Id { get; set; }
        public string Parameter { get; set; } = default!;
        public string? AcceptanceCriteriaValue { get; set; }
        public bool Numeric { get; set; }
        public decimal? MinimumValue { get; set; }
        public decimal? MaximumValue { get; set; }
    }

    public sealed class InspectionTemplateDto
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = default!;
        public List<TemplateParameterDto> Parameters { get; set; } = new();
        public int IsActive { get; set; }
    }

    public sealed class TemplateListItemDto
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = default!;
        public int ParameterCount { get; set; }
        public int IsActive { get; set; }
    }

    public sealed class TemplateAutoCompleteDto
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = default!;
    }

    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}
