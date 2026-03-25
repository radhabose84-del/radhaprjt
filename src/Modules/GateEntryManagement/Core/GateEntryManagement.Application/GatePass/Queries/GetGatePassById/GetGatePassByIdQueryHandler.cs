using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Queries.GetGatePassById
{
    public class GetGatePassByIdQueryHandler : IRequestHandler<GetGatePassByIdQuery, GatePassHdrDto?>
    {
        private readonly IGatePassQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetGatePassByIdQueryHandler(IGatePassQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<GatePassHdrDto?> Handle(GetGatePassByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetGatePassByIdQuery",
                actionName: result.Id.ToString(),
                details: $"GatePass details {result.Id} was fetched.",
                module: "GatePass"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
