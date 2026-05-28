using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityParameter.Queries.GetQualityParameterById
{
    public class GetQualityParameterByIdQueryHandler : IRequestHandler<GetQualityParameterByIdQuery, QualityParameterDto?>
    {
        private readonly IQualityParameterQueryRepository _queryRepository;
        private readonly IUOMLookup _uomLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetQualityParameterByIdQueryHandler(
            IQualityParameterQueryRepository queryRepository,
            IUOMLookup uomLookup,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _uomLookup = uomLookup;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<QualityParameterDto?> Handle(GetQualityParameterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<QualityParameterDto>(result);

            if (dto.UnitId.HasValue)
            {
                var uoms = await _uomLookup.GetByIdsAsync(new[] { dto.UnitId.Value }, cancellationToken);
                var uom = uoms.FirstOrDefault();
                if (uom != null)
                {
                    dto.UnitCode = uom.Code;
                    dto.UnitName = uom.UOMName;
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetQualityParameterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"QualityParameter details {dto.Id} was fetched.",
                module: "QualityParameter"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
