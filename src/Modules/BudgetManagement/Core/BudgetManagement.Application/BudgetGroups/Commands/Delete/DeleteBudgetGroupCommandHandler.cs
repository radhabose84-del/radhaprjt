using Contracts.Common;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup
{
    public class DeleteBudgetGroupCommandHandler : IRequestHandler<DeleteBudgetGroupCommand, int>
    {
        private readonly IBudgetGroupCommandRepository _commandRepo;
        private readonly IBudgetGroupQueryRepository _queryRepo;
        private readonly IMediator _mediator;

        public DeleteBudgetGroupCommandHandler(
            IBudgetGroupCommandRepository commandRepo,
            IBudgetGroupQueryRepository queryRepo,
            IMediator mediator)
        {
            _commandRepo = commandRepo;
            _queryRepo   = queryRepo;
            _mediator    = mediator;
        }

        public async Task<int> Handle(DeleteBudgetGroupCommand request, CancellationToken cancellationToken)
        {
            // 1) Load existing entity
            var entity = await _commandRepo.GetByIdAsync(request.Id, cancellationToken);

            if (entity == null || entity.IsDeleted == BaseEntity.IsDelete.Deleted)
            {
                throw new ExceptionRules("Budget Group not found or already deleted.");
            }

            // 2) Check if it is used as ParentBudgetGroupId by any active group
            var hasChildren = await _queryRepo.SoftDeleteValidation(request.Id, cancellationToken);
            if (hasChildren)
            {
                throw new ExceptionRules("Cannot delete this Budget Group because it is used as a Parent Budget Group.");
            }

            // 3) Soft delete
            var success = await _commandRepo.SoftDeleteAsync(request.Id, cancellationToken);

            if (!success)
            {
                // If we reach here, something is really wrong (e.g. DB update failed)
                throw new ExceptionRules("BudgetGroup deletion failed.");
            }

            // 4) Audit log
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "DeleteBudgetGroup",
                actionCode: request.Id.ToString(),
                actionName: entity.Name ?? string.Empty,
                details: "Budget Group was soft deleted.",
                module: "BudgetGroup");

            await _mediator.Publish(domainEvent, cancellationToken);

            return request.Id;
        }
    }
}
