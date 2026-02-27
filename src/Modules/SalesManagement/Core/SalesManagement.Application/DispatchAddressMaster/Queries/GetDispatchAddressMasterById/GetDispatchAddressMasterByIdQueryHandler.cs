using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMaster.Queries.GetDispatchAddressMasterById
{
    public class GetDispatchAddressMasterByIdQueryHandler : IRequestHandler<GetDispatchAddressMasterByIdQuery, DispatchAddressMasterDto?>
    {
        private readonly IDispatchAddressMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDispatchAddressMasterByIdQueryHandler(IDispatchAddressMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<DispatchAddressMasterDto?> Handle(GetDispatchAddressMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<DispatchAddressMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetDispatchAddressMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"DispatchAddressMaster details {dto.Id} was fetched.",
                module: "DispatchAddressMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
