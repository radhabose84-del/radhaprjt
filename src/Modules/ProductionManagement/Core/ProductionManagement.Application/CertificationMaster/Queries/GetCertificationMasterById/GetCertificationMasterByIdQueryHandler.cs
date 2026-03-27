using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CertificationMaster.Queries.GetCertificationMasterById
{
    public class GetCertificationMasterByIdQueryHandler : IRequestHandler<GetCertificationMasterByIdQuery, CertificationMasterDto>
    {
        private readonly ICertificationMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCertificationMasterByIdQueryHandler(
            ICertificationMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<CertificationMasterDto> Handle(GetCertificationMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var dto = _mapper.Map<CertificationMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetCertificationMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Certification Master details {dto.Id} was fetched.",
                module: "CertificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
