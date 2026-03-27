using MediatR;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.QualityMaster.Commands.DeleteQualityMaster
{
    public class DeleteQualityMasterCommandHandler : IRequestHandler<DeleteQualityMasterCommand, bool>
    {
        private readonly IQualityMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteQualityMasterCommandHandler(
            IQualityMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteQualityMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "QUALITYMASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Quality Master with Id {request.Id} soft deleted.",
                module: "QualityMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
