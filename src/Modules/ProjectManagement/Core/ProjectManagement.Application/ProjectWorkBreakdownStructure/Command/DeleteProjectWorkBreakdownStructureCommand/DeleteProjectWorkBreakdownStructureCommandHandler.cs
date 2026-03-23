using Contracts.Common;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand;
using ProjectManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.DeleteProjectWorkBreakdownStructureCommand
{
    public class DeleteProjectWorkBreakdownStructureCommandHandler
        : IRequestHandler<SoftDeleteProjectWorkBreakdownStructureCommand.DeleteProjectWorkBreakdownStructureCommand, bool>
    {
        private readonly IProjectWorkBreakdownStructureCommandRepository _commandRepo;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteProjectWorkBreakdownStructureCommandHandler> _logger;

        public DeleteProjectWorkBreakdownStructureCommandHandler(
            IProjectWorkBreakdownStructureCommandRepository commandRepo,
            IMediator mediator,
            ILogger<DeleteProjectWorkBreakdownStructureCommandHandler> logger)
        {
            _commandRepo = commandRepo;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<bool> Handle(
            SoftDeleteProjectWorkBreakdownStructureCommand.DeleteProjectWorkBreakdownStructureCommand request,
            CancellationToken cancellationToken)
        {
            var isDeleted = await _commandRepo.DeleteAsync(request.Id);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "WBS_DELETE",
                actionName: request.Id.ToString(),
                details: isDeleted
                    ? $"WBS with Id {request.Id} was soft deleted."
                    : $"Soft delete failed for WBS with Id {request.Id}.",
                module: "ProjectWorkBreakdownStructure"), cancellationToken);

            if (!isDeleted)
                throw new ExceptionRules("WBS deletion failed. Record not found.");

            _logger.LogInformation("WBS deleted. Id={WbsId}", request.Id);
            return true;
        }
    }
}
