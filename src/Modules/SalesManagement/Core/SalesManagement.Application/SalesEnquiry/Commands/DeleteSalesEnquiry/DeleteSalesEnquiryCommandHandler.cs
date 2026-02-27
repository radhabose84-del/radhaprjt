using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesEnquiry.Commands.DeleteSalesEnquiry
{
    public class DeleteSalesEnquiryCommandHandler : IRequestHandler<DeleteSalesEnquiryCommand, bool>
    {
        private readonly ISalesEnquiryCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesEnquiryCommandHandler(
            ISalesEnquiryCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesEnquiryCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Sales Enquiry not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALESENQUIRY_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Enquiry with Id {request.Id} soft deleted.",
                module: "SalesEnquiry");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
