using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DiscountMaster.Commands.DeleteDiscountMaster
{
    public class DeleteDiscountMasterCommandHandler : IRequestHandler<DeleteDiscountMasterCommand, bool>
    {
        private readonly IDiscountMasterCommandRepository _commandRepository;
        private readonly IDiscountMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteDiscountMasterCommandHandler(
            IDiscountMasterCommandRepository commandRepository,
            IDiscountMasterQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteDiscountMasterCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("DiscountMaster not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "DISCOUNT_MASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"DiscountMaster with Id {request.Id} soft deleted successfully.",
                module: "DiscountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
