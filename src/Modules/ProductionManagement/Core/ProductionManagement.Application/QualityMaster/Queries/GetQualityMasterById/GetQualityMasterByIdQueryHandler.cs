using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.QualityMaster.Queries.GetQualityMasterById
{
    public class GetQualityMasterByIdQueryHandler : IRequestHandler<GetQualityMasterByIdQuery, QualityMasterDto>
    {
        private readonly IQualityMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetQualityMasterByIdQueryHandler(
            IQualityMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<QualityMasterDto> Handle(GetQualityMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var dto = _mapper.Map<QualityMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetQualityMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Quality Master details {dto.Id} was fetched.",
                module: "QualityMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
