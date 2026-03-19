using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Application.UsageType.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UsageType.Queries.GetUsageTypeById
{
    public class GetUsageTypeByIdQueryHandler : IRequestHandler<GetUsageTypeByIdQuery, UsageTypeDto?>
    {
        private readonly IUsageTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetUsageTypeByIdQueryHandler(IUsageTypeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<UsageTypeDto?> Handle(GetUsageTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<UsageTypeDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetUsageTypeByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"UsageType details {dto.Id} was fetched.",
                module: "UsageType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
