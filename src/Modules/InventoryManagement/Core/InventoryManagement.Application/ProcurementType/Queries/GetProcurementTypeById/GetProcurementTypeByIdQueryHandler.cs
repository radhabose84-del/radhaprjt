using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Application.ProcurementType.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ProcurementType.Queries.GetProcurementTypeById
{
    public class GetProcurementTypeByIdQueryHandler : IRequestHandler<GetProcurementTypeByIdQuery, ProcurementTypeDto?>
    {
        private readonly IProcurementTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProcurementTypeByIdQueryHandler(IProcurementTypeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ProcurementTypeDto?> Handle(GetProcurementTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<ProcurementTypeDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetProcurementTypeByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"ProcurementType details {dto.Id} was fetched.",
                module: "ProcurementType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
