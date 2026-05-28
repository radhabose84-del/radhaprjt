using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorRatingGrade.Commands.DeleteVendorRatingGrade
{
    public class DeleteVendorRatingGradeCommandHandler : IRequestHandler<DeleteVendorRatingGradeCommand, bool>
    {
        private readonly IVendorRatingGradeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteVendorRatingGradeCommandHandler(
            IVendorRatingGradeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteVendorRatingGradeCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);
            if (!result)
                throw new ExceptionRules("VendorRatingGrade not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "VENDOR_RATING_GRADE_DELETE",
                actionName: request.Id.ToString(),
                details: $"VendorRatingGrade with Id {request.Id} deleted successfully.",
                module: "VendorRatingGrade"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
