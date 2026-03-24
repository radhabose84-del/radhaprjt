using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQueryHandler : IRequestHandler<GetMiscTypeMasterByIdQuery, MiscTypeMasterDto?>
    {
        private readonly IMiscTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMiscTypeMasterByIdQueryHandler(IMiscTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MiscTypeMasterDto?> Handle(GetMiscTypeMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetMiscTypeMasterByIdQuery",
                actionName: result.Id.ToString(),
                details: $"MiscTypeMaster details {result.Id} was fetched.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
