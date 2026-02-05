// GetAll handler
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.DTOs;
using MediatR;

namespace InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplates
{
    public sealed class GetAllTemplatesQueryHandler
        : IRequestHandler<GetAllTemplatesQuery, PagedResult<TemplateListItemDto>>
    {
        private readonly ITemplateQueryRepository _qry;
        private readonly IMapper _mapper;

        public GetAllTemplatesQueryHandler(ITemplateQueryRepository qry, IMapper mapper)
        { _qry = qry; _mapper = mapper; }


        public async Task<PagedResult<TemplateListItemDto>> Handle(GetAllTemplatesQuery request, CancellationToken ct)
        {
            var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var size = request.PageSize <= 0 ? 20 : request.PageSize;

            var (items, total) = await _qry.GetAllAsync(request.SearchTerm?.Trim(), page, size, ct);
            var dto = items.Select(i => _mapper.Map<TemplateListItemDto>(i)).ToList();

            return new PagedResult<TemplateListItemDto>
            {
                Items = dto,
                TotalCount = total,
                Page = page,
                PageSize = size
            };
        }
    }
}
