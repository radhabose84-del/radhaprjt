using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Commands.DeleteVoucherType
{
    public class DeleteVoucherTypeCommandHandler : IRequestHandler<DeleteVoucherTypeCommand, bool>
    {
        private readonly IVoucherTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteVoucherTypeCommandHandler(
            IVoucherTypeMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteVoucherTypeCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "VOUCHER_TYPE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Voucher Type with Id {request.Id} soft deleted.",
                module: "VoucherType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
