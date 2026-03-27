using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Queries.GetGateInwardById
{
    public class GetGateInwardByIdQueryHandler : IRequestHandler<GetGateInwardByIdQuery, GateInwardHdrDto?>
    {
        private readonly IGateInwardQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetGateInwardByIdQueryHandler(IGateInwardQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<GateInwardHdrDto?> Handle(GetGateInwardByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);
            if (result == null) return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById", actionCode: "GetGateInwardByIdQuery",
                actionName: result.Id.ToString(), details: $"GateInward details {result.Id} was fetched.", module: "GateInward");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
