using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityTemplate.Commands.DeleteQualityTemplate
{
    public class DeleteQualityTemplateCommandHandler : IRequestHandler<DeleteQualityTemplateCommand, bool>
    {
        private readonly IQualityTemplateCommandRepository _commandRepository;
        private readonly IQualityTemplateQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteQualityTemplateCommandHandler(
            IQualityTemplateCommandRepository commandRepository,
            IQualityTemplateQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteQualityTemplateCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Quality Template not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "QUALITY_TEMPLATE_DELETE",
                actionName: request.Id.ToString(),
                details: $"QualityTemplate with Id {request.Id} soft deleted successfully.",
                module: "QualityTemplate"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
