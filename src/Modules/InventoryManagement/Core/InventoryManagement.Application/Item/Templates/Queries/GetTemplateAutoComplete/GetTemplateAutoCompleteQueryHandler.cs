using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.DTOs;
using InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplateAutoComplete;
using MediatR;

namespace InventoryManagement.Application.Item.Templates.Queries.GetTemplateAutoComplete
{
    public sealed class GetTemplateAutoCompleteQueryHandler
        : IRequestHandler<GetTemplateAutoCompleteQuery, List<TemplateAutoCompleteDto>>
    {
        private readonly ITemplateQueryRepository _qry;

        public GetTemplateAutoCompleteQueryHandler(ITemplateQueryRepository qry) => _qry = qry;

        public async Task<List<TemplateAutoCompleteDto>> Handle(GetTemplateAutoCompleteQuery request, CancellationToken ct)
        {
            var take = request.Take <= 0 ? 10 : request.Take;
            var items = await _qry.GetAutoCompleteAsync(request.SearchPattern?.Trim(), take, ct);

            return items.Select(x => new TemplateAutoCompleteDto
            {
                Id = x.Id,
                TemplateName = x.TemplateName
            }).ToList();
        }
    }
}
