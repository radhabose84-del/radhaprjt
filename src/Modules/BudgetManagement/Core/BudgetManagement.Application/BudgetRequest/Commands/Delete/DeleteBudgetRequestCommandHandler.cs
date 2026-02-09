using BudgetManagement.Application.BudgetRequest.Commands.Delete;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Domain.Events;
using MediatR;

public class DeleteBudgetRequestCommandHandler 
    : IRequestHandler<DeleteBudgetRequestCommand>
{
    private readonly IBudgetRequestCommandRepository _budgetRepo;
    private readonly IMediator _mediator;

    public DeleteBudgetRequestCommandHandler(
        IBudgetRequestCommandRepository budgetRepo,
        IMediator mediator)
    {
        _budgetRepo = budgetRepo;
        _mediator   = mediator;
    }

    public async Task Handle(DeleteBudgetRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _budgetRepo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null)
            return;

        await _budgetRepo.SoftDeleteAsync(request.Id, cancellationToken);

        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Delete",
            actionCode:   entity.RequestCode ?? entity.Id.ToString(),
            actionName:   "Budget Request",
            details:      $"Budget Request '{entity.RequestCode}' was deleted.",
            module:       "BudgetRequest"
        );

        await _mediator.Publish(domainEvent, cancellationToken);
    }
}
