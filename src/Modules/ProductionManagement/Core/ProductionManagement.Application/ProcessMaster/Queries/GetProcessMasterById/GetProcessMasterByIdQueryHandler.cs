using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.ProcessMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProcessMaster.Queries.GetProcessMasterById
{
    public class GetProcessMasterByIdQueryHandler : IRequestHandler<GetProcessMasterByIdQuery, ProcessMasterDto>
    {
        private readonly IProcessMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProcessMasterByIdQueryHandler(
            IProcessMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ProcessMasterDto> Handle(GetProcessMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var dto = _mapper.Map<ProcessMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetProcessMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Process Master details {dto.Id} was fetched.",
                module: "ProcessMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
