using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualitySpecification.Commands.DeleteQualitySpecification
{
    public class DeleteQualitySpecificationCommandHandler : IRequestHandler<DeleteQualitySpecificationCommand, bool>
    {
        private readonly IQualitySpecificationCommandRepository _commandRepository;
        private readonly IQualitySpecificationQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteQualitySpecificationCommandHandler(
            IQualitySpecificationCommandRepository commandRepository,
            IQualitySpecificationQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteQualitySpecificationCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Quality Specification not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "QUALITY_SPECIFICATION_DELETE",
                actionName: request.Id.ToString(),
                details: $"QualitySpecification with Id {request.Id} soft deleted successfully.",
                module: "QualitySpecification"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
