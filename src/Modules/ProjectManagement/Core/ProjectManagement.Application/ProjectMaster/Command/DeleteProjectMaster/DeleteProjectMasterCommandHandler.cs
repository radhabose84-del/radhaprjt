using AutoMapper;
using Contracts.Common;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.Application.ProjectMaster.Command.DeleteProjectMaster
{
    public class DeleteProjectMasterCommandHandler : IRequestHandler<DeleteProjectMasterCommand, bool>
    {
        private readonly IProjectMasterCommandRepository _commandRepo;
        private readonly ILogger<DeleteProjectMasterCommandHandler> _logger;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;

        public DeleteProjectMasterCommandHandler(IProjectMasterCommandRepository commandRepo, ILogger<DeleteProjectMasterCommandHandler> logger, IMapper imapper, IMediator mediator)
        {
            _commandRepo = commandRepo;
            _logger = logger;
            _imapper = imapper;
            _mediator = mediator;
        }

         public async Task<bool> Handle( DeleteProjectMasterCommand request,   CancellationToken cancellationToken)
        {
            
            var isDeleted = await _commandRepo.SoftDeleteAsync(request.Id, cancellationToken);
           
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: request.Id.ToString(),
                actionName: isDeleted ? "SoftDeleted" : "DeleteFailed",
                details: isDeleted
                    ? $"ProjectMaster with ID {request.Id} was soft deleted."
                    : $"Soft delete failed for ProjectMaster with ID {request.Id}.",
                module: "ProjectMaster"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

           
            if (!isDeleted)
                throw new ExceptionRules("ProjectMaster deletion failed.");

           
            return true;
        }
    }
}