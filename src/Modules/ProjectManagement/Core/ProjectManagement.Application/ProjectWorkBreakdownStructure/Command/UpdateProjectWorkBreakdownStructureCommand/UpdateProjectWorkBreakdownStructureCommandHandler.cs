using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.UpdateProjectWorkBreakdownStructureCommand
{
    public class UpdateProjectWorkBreakdownStructureCommandHandler : IRequestHandler<UpdateProjectWorkBreakdownStructureCommand, ProjectWorkBreakdownStructureDto>
    {
        private readonly IProjectWorkBreakdownStructureCommandRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateProjectWorkBreakdownStructureCommandHandler> _logger;

        public UpdateProjectWorkBreakdownStructureCommandHandler(
            IProjectWorkBreakdownStructureCommandRepository repository,
            IMapper mapper,
            IMediator mediator,
            ILogger<UpdateProjectWorkBreakdownStructureCommandHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

       public async Task<ProjectWorkBreakdownStructureDto> Handle(
            UpdateProjectWorkBreakdownStructureCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Validate project
            var project = await _repository.GetProjectAsync(request.ProjectId);
            if (project == null)
                throw new ArgumentException("Invalid ProjectId.");

            // 2️⃣ Load existing WBS
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity == null)
                throw new ArgumentException("WBS record not found.");

            if (entity.ProjectId != request.ProjectId)
                throw new ArgumentException("WBS does not belong to the given project.");

            // 3️⃣ Validate parent (if provided)
            int level = 1;
            if (request.ParentWorkBreakdownStructureId.HasValue)
            {
                var parentId = request.ParentWorkBreakdownStructureId.Value;

                if (parentId == request.Id)
                    throw new ArgumentException("Parent WBS cannot be same as current WBS.");

                var parent = await _repository.GetByIdAsync(parentId);
                if (parent == null || parent.ProjectId != request.ProjectId)
                {
                    _logger.LogError(
                        "Invalid Parent WBS. ParentId={ParentId}, ProjectId={ProjectId}",
                        parentId, request.ProjectId);

                    throw new ArgumentException("Invalid Parent WBS.");
                }

                level = parent.Level + 1;
            }

            // 4️⃣ Use AutoMapper to update entity from command
            //    (respects ignores configured in profile)
            _mapper.Map(request, entity);

            // 5️⃣ Override fields that come from Project / calculated logic
            entity.UnitId       = project.UnitId;
            entity.BudgetYearId = project.BudgetYearId;
            entity.CurrencyId   = project.CurrencyId;
            entity.Level        = level;

            // 6️⃣ Duration calculation
            if (entity.StartDate.HasValue && entity.EndDate.HasValue)
            {
                entity.DurationInDays = (int)(entity.EndDate.Value - entity.StartDate.Value).TotalDays;
            }
            else
            {
                entity.DurationInDays = null;
            }

            // 7️⃣ Save
            await _repository.UpdateAsync(entity);

            // 8️⃣ Audit log
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "WBS_UPDATE",
                actionName: $"Project WBS '{entity.WorkBreakdownStructureName}' updated",
                details: $"WBS Id {entity.Id} updated for ProjectId {entity.ProjectId}.",
                module: "ProjectWorkBreakdownStructure");

            await _mediator.Publish(domainEvent, cancellationToken);

            // 9️⃣ Map to DTO
            var dto = _mapper.Map<ProjectWorkBreakdownStructureDto>(entity);
            dto.ProjectId   = project.Id;
            dto.ProjectCode = project.ProjectCode;

            return dto;
        }
    }
}