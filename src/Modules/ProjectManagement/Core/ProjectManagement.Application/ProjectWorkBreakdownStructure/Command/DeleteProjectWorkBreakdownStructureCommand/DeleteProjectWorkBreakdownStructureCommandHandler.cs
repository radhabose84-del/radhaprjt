using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand
{
    public class DeleteProjectWorkBreakdownStructureCommandHandler     : IRequestHandler<DeleteProjectWorkBreakdownStructureCommand, bool>
    {
        private readonly IProjectWorkBreakdownStructureCommandRepository _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteProjectWorkBreakdownStructureCommandHandler> _logger;

        public DeleteProjectWorkBreakdownStructureCommandHandler(
            IProjectWorkBreakdownStructureCommandRepository repository,
            IMediator mediator,
            ILogger<DeleteProjectWorkBreakdownStructureCommandHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<bool> Handle(
            DeleteProjectWorkBreakdownStructureCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Load entity first (for logging + audit)
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                _logger.LogWarning("WBS soft delete failed. Id {Id} not found.", request.Id);
                return false;
            }

            // 2️⃣ Perform soft delete
            var deleted = await _repository.DeleteAsync(request.Id);
            if (!deleted)
            {
                _logger.LogWarning("WBS soft delete failed at repository. Id {Id}", request.Id);
                return false;
            }

            // 3️⃣ Audit log event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "WBS_DELETE",
                actionName: $"Project WBS '{entity.WorkBreakdownStructureName}' deleted",
                details: $"WBS Id {entity.Id} for ProjectId {entity.ProjectId} soft-deleted.",
                module: "ProjectWorkBreakdownStructure"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return true;
        }
    }
}