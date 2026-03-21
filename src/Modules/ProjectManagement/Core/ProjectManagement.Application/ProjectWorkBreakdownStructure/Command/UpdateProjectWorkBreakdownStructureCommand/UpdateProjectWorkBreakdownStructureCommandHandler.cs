using AutoMapper;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.UpdateProjectWorkBreakdownStructureCommand
{
    public class UpdateProjectWorkBreakdownStructureCommandHandler
        : IRequestHandler<UpdateProjectWorkBreakdownStructureCommand, ProjectWorkBreakdownStructureDto>
    {
        private readonly IProjectWorkBreakdownStructureCommandRepository _commandRepo;
        private readonly IProjectWorkBreakdownStructureQueryRepository _queryRepo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateProjectWorkBreakdownStructureCommandHandler> _logger;

        public UpdateProjectWorkBreakdownStructureCommandHandler(
            IProjectWorkBreakdownStructureCommandRepository commandRepo,
            IProjectWorkBreakdownStructureQueryRepository queryRepo,
            IMapper mapper,
            IMediator mediator,
            ILogger<UpdateProjectWorkBreakdownStructureCommandHandler> logger)
        {
            _commandRepo = commandRepo;
            _queryRepo = queryRepo;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ProjectWorkBreakdownStructureDto> Handle(
            UpdateProjectWorkBreakdownStructureCommand request,
            CancellationToken cancellationToken)
        {
            // 1) Fetch existing
            var entity = await _commandRepo.GetByIdAsync(request.Id);
            if (entity == null)
                throw new Exception($"WBS with Id {request.Id} not found.");

            // 2) Map updated fields (AutoMapper ignores Id, ProjectId, Level, etc.)
            _mapper.Map(request, entity);

            // 3) Recalculate level if parent changed
            if (request.ParentWorkBreakdownStructureId.HasValue && request.ParentWorkBreakdownStructureId > 0)
            {
                var parentLevel = await _queryRepo.GetParentLevelAsync(request.ParentWorkBreakdownStructureId.Value);
                entity.Level = parentLevel + 1;
            }
            else
            {
                entity.Level = 1;
            }

            // 4) Recalculate duration
            if (entity.StartDate.HasValue && entity.EndDate.HasValue)
            {
                entity.DurationInDays = (int)(entity.EndDate.Value - entity.StartDate.Value).TotalDays;
            }

            // 5) Persist
            await _commandRepo.UpdateAsync(entity);

            // 6) Audit log
            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "WBS_UPDATE",
                actionName: request.Id.ToString(),
                details: $"WBS with Id {request.Id} updated for ProjectId {entity.ProjectId}.",
                module: "ProjectWorkBreakdownStructure"), cancellationToken);

            _logger.LogInformation("WBS updated. Id={WbsId}, ProjectId={ProjectId}", request.Id, entity.ProjectId);

            // 7) Reload full DTO
            var dto = await _queryRepo.GetByIdAsync(request.Id);
            return dto ?? _mapper.Map<ProjectWorkBreakdownStructureDto>(entity);
        }
    }
}
