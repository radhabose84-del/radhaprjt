using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.DTOs;
using MediatR;

namespace InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplateById
{
    public sealed class GetInspectionTemplateByIdQueryHandler
        : IRequestHandler<GetInspectionTemplateByIdQuery, InspectionTemplateDto?>
    {
        private readonly ITemplateQueryRepository _qry;
        private readonly IMapper _mapper;

        public GetInspectionTemplateByIdQueryHandler(ITemplateQueryRepository qry, IMapper mapper)
        { _qry = qry; _mapper = mapper; }

        public async Task<InspectionTemplateDto?> Handle(GetInspectionTemplateByIdQuery request, CancellationToken ct)
        {
            var entity = await _qry.GetByIdAsync(request.Id, ct);
            return entity is null ? null : _mapper.Map<InspectionTemplateDto>(entity);
        }
    }
}
