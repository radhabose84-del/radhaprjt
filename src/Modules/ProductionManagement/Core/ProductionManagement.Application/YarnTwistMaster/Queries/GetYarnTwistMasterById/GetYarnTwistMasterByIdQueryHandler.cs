using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnTwistMaster.Queries.GetYarnTwistMasterById
{
    public class GetYarnTwistMasterByIdQueryHandler : IRequestHandler<GetYarnTwistMasterByIdQuery, YarnTwistMasterDto>
    {
        private readonly IYarnTwistMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetYarnTwistMasterByIdQueryHandler(
            IYarnTwistMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<YarnTwistMasterDto> Handle(GetYarnTwistMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var dto = _mapper.Map<YarnTwistMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetYarnTwistMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Yarn Twist Master details {dto.Id} was fetched.",
                module: "YarnTwistMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
