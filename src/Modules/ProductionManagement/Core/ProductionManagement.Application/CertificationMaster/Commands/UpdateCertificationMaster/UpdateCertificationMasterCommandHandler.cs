using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CertificationMaster.Commands.UpdateCertificationMaster
{
    public class UpdateCertificationMasterCommandHandler : IRequestHandler<UpdateCertificationMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ICertificationMasterCommandRepository _commandRepository;
        private readonly ICertificationMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateCertificationMasterCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateCertificationMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.CertificationMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "CERTIFICATIONMASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Certification Master with Id {request.Id} updated successfully.",
                module: "CertificationMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Certification Master updated successfully.",
                Data = result
            };
        }
    }
}
