using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QcInspection.Commands.DeleteQcInspection
{
    public class DeleteQcInspectionCommandHandler : IRequestHandler<DeleteQcInspectionCommand, bool>
    {
        private readonly IQcInspectionCommandRepository _commandRepository;
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteQcInspectionCommandHandler(
            IQcInspectionCommandRepository commandRepository,
            IQcInspectionQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteQcInspectionCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("QC Inspection not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "QC_INSPECTION_DELETE",
                actionName: request.Id.ToString(),
                details: $"QC Inspection with Id {request.Id} soft deleted successfully.",
                module: "QcInspection"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
