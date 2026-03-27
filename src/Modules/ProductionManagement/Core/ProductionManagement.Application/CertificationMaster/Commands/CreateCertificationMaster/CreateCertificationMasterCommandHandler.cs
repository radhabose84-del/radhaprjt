using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CertificationMaster.Commands.CreateCertificationMaster
{
    public class CreateCertificationMasterCommandHandler : IRequestHandler<CreateCertificationMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ICertificationMasterCommandRepository _commandRepository;
        private readonly ICertificationMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateCertificationMasterCommandHandler(
            ICertificationMasterCommandRepository commandRepository,
            ICertificationMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateCertificationMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.CertificationMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "CERTIFICATIONMASTER_CREATE",
                actionName: request.CertificationName ?? string.Empty,
                details: $"Certification Master '{request.CertificationName}' created successfully with Id {newId}.",
                module: "CertificationMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Certification Master created successfully.",
                Data = newId
            };
        }
    }
}
