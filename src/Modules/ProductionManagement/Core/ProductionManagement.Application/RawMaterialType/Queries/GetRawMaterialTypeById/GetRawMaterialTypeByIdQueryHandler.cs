using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RawMaterialType.Queries.GetRawMaterialTypeById
{
    public class GetRawMaterialTypeByIdQueryHandler : IRequestHandler<GetRawMaterialTypeByIdQuery, RawMaterialTypeDto>
    {
        private readonly IRawMaterialTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetRawMaterialTypeByIdQueryHandler(
            IRawMaterialTypeQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<RawMaterialTypeDto> Handle(GetRawMaterialTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var dto = _mapper.Map<RawMaterialTypeDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetRawMaterialTypeByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Raw Material Type details {dto.Id} was fetched.",
                module: "RawMaterialType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
