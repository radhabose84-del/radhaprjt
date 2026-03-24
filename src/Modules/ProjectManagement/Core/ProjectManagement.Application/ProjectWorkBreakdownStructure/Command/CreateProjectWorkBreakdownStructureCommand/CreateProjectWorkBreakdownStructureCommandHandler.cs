using AutoMapper;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand
{
    public class CreateProjectWorkBreakdownStructureCommandHandler
        : IRequestHandler<CreateProjectWorkBreakdownStructureCommand, ProjectWorkBreakdownStructureDto>
    {
        private readonly IProjectWorkBreakdownStructureCommandRepository _commandRepo;
        private readonly IProjectWorkBreakdownStructureQueryRepository _queryRepo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateProjectWorkBreakdownStructureCommandHandler> _logger;

        public CreateProjectWorkBreakdownStructureCommandHandler(
            IProjectWorkBreakdownStructureCommandRepository commandRepo,
            IProjectWorkBreakdownStructureQueryRepository queryRepo,
            IMapper mapper,
            IMediator mediator,
            ILogger<CreateProjectWorkBreakdownStructureCommandHandler> logger)
        {
            _commandRepo = commandRepo;
            _queryRepo = queryRepo;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ProjectWorkBreakdownStructureDto> Handle(
            CreateProjectWorkBreakdownStructureCommand request,
            CancellationToken cancellationToken)
        {
            // 1) Map command → entity
            var entity = _mapper.Map<Domain.Entities.ProjectWorkBreakdownStructure>(request);

            // 2) Calculate level based on parent
            if (request.ParentWorkBreakdownStructureId.HasValue && request.ParentWorkBreakdownStructureId > 0)
            {
                var parentLevel = await _queryRepo.GetParentLevelAsync(request.ParentWorkBreakdownStructureId.Value);
                entity.Level = parentLevel + 1;
            }
            else
            {
                entity.Level = 1;
            }

            // 3) Calculate duration
            if (entity.StartDate.HasValue && entity.EndDate.HasValue)
            {
                entity.DurationInDays = (int)(entity.EndDate.Value - entity.StartDate.Value).TotalDays;
            }

            // 4) Persist
            await _commandRepo.AddAsync(entity);

            // 5) Audit log
            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "WBS_CREATE",
                actionName: entity.WorkBreakdownStructureName ?? entity.Id.ToString(),
                details: $"WBS '{entity.WorkBreakdownStructureName}' created with Id {entity.Id} for ProjectId {entity.ProjectId}.",
                module: "ProjectWorkBreakdownStructure"), cancellationToken);

            _logger.LogInformation("WBS created. Id={WbsId}, ProjectId={ProjectId}", entity.Id, entity.ProjectId);

            // 6) Reload full DTO
            var dto = await _queryRepo.GetByIdAsync(entity.Id);
            return dto ?? _mapper.Map<ProjectWorkBreakdownStructureDto>(entity);
        }
    }
}
