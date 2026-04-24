using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.TripSheet.Commands.DeleteTripSheet
{
    public class DeleteTripSheetCommandHandler : IRequestHandler<DeleteTripSheetCommand, bool>
    {
        private readonly ITripSheetCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteTripSheetCommandHandler(
            ITripSheetCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteTripSheetCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Trip Sheet not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "TRIPSHEET_DELETE",
                actionName: request.Id.ToString(),
                details: $"Trip Sheet with Id {request.Id} deleted successfully.",
                module: "TripSheet"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
