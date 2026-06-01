using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityTemplate.Queries.GetQualityTemplateById
{
    public class GetQualityTemplateByIdQueryHandler : IRequestHandler<GetQualityTemplateByIdQuery, QualityTemplateDto?>
    {
        private readonly IQualityTemplateQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IUOMLookup _uomLookup;

        public GetQualityTemplateByIdQueryHandler(
            IQualityTemplateQueryRepository queryRepository,
            IMediator mediator,
            IUOMLookup uomLookup)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
            _uomLookup = uomLookup;
        }

        public async Task<QualityTemplateDto?> Handle(GetQualityTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _queryRepository.GetByIdAsync(request.Id);

            if (dto == null)
                return null;

            // Populate ParameterUnitCode/Name from cross-module UOM lookup
            if (dto.Parameters != null && dto.Parameters.Count > 0)
            {
                var unitIds = dto.Parameters
                    .Where(p => p.ParameterUnitId.HasValue && p.ParameterUnitId.Value > 0)
                    .Select(p => p.ParameterUnitId!.Value)
                    .Distinct()
                    .ToList();

                if (unitIds.Count > 0)
                {
                    var uoms = await _uomLookup.GetByIdsAsync(unitIds, cancellationToken);
                    var uomMap = uoms.ToDictionary(u => u.Id);
                    foreach (var p in dto.Parameters.Where(x => x.ParameterUnitId.HasValue))
                    {
                        if (uomMap.TryGetValue(p.ParameterUnitId!.Value, out var uom))
                        {
                            p.ParameterUnitCode = uom.Code;
                            p.ParameterUnitName = uom.UOMName;
                        }
                    }
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetQualityTemplateByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"QualityTemplate details {dto.Id} was fetched.",
                module: "QualityTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
