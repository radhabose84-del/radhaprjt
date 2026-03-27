using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CertificationMaster.Commands.DeleteCertificationMaster
{
    public class DeleteCertificationMasterCommandHandler : IRequestHandler<DeleteCertificationMasterCommand, bool>
    {
        private readonly ICertificationMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteCertificationMasterCommandHandler(
            ICertificationMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteCertificationMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "CERTIFICATIONMASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Certification Master with Id {request.Id} soft deleted.",
                module: "CertificationMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
