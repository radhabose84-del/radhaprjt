using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete
{
    public class GetItemAutoCompleteQuery : IRequest<List<GetItemAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
        public int? ItemGroupId { get; set; }
        public int? ItemCategoryId { get; set; }
        public int? SourceId { get; set; }
        public int? IssueRuleId { get; set; }
        public int? ModuleId { get; set; }
        public int? SalesGroupId { get; set; }
    }
}