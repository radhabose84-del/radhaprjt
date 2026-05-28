using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityParameter.Commands.DeleteQualityParameter
{
    public class DeleteQualityParameterCommandHandler : IRequestHandler<DeleteQualityParameterCommand, bool>
    {
        private readonly IQualityParameterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteQualityParameterCommandHandler(
            IQualityParameterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteQualityParameterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "QUALITY_PARAMETER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Quality Parameter with Id {request.Id} soft deleted.",
                module: "QualityParameter"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
