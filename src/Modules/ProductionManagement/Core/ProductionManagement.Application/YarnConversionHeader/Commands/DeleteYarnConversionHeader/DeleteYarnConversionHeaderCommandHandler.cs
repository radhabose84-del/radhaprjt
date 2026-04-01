using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnConversionHeader.Commands.DeleteYarnConversionHeader
{
    public class DeleteYarnConversionHeaderCommandHandler
        : IRequestHandler<DeleteYarnConversionHeaderCommand, bool>
    {
        private readonly IYarnConversionHeaderCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteYarnConversionHeaderCommandHandler(
            IYarnConversionHeaderCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(
            DeleteYarnConversionHeaderCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Yarn Conversion not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "YARN_CONVERSION_DELETE",
                actionName: request.Id.ToString(),
                details: $"Yarn Conversion with Id {request.Id} soft deleted successfully.",
                module: "YarnConversionHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
