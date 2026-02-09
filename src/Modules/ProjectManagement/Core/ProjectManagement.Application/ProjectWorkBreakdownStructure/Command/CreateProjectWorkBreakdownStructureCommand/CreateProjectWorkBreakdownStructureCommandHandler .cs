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

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand
{
    public class CreateProjectWorkBreakdownStructureCommandHandler : IRequestHandler<CreateProjectWorkBreakdownStructureCommand, ProjectWorkBreakdownStructureDto>
    {
        private readonly IProjectWorkBreakdownStructureCommandRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateProjectWorkBreakdownStructureCommandHandler> _logger;

        public CreateProjectWorkBreakdownStructureCommandHandler(
            IProjectWorkBreakdownStructureCommandRepository repository,
            IMapper mapper,
            IMediator mediator,
            ILogger<CreateProjectWorkBreakdownStructureCommandHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ProjectWorkBreakdownStructureDto> Handle(
            CreateProjectWorkBreakdownStructureCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Validate Project
            var project = await _repository.GetProjectAsync(request.ProjectId);
            if (project == null)
            {
                throw new ArgumentException("Invalid ProjectId.");
            }

            // Optional: validate dates within Project's Start/End
            if (request.StartDate.HasValue && request.StartDate < project.StartDate)
                throw new ArgumentException("WBS Start Date cannot be earlier than Project Start Date.");

            if (request.EndDate.HasValue && request.EndDate > project.EndDate)
                throw new ArgumentException("WBS End Date cannot be later than Project End Date.");

            // 2️⃣ Parent WBS / Level
            var level = 1;
            if (request.ParentWorkBreakdownStructureId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(request.ParentWorkBreakdownStructureId.Value);
                if (parent == null || parent.ProjectId != request.ProjectId)
                {
                    _logger.LogError(
                        "Parent WBS not found or belongs to another project. ParentId: {ParentId}, ProjectId: {ProjectId}",
                        request.ParentWorkBreakdownStructureId.Value,
                        request.ProjectId);

                    throw new ArgumentException("Invalid Parent WBS.");
                }

                level = parent.Level + 1;
            }

            // 3️⃣ Map command → entity
            var entity = _mapper.Map<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure>(request);

            // Inherit from Project master
            entity.UnitId = project.UnitId;
            entity.BudgetYearId = project.BudgetYearId;
            entity.CurrencyId = project.CurrencyId;  // default from project as per requirement
            entity.Level = level;

            // 4️⃣ Duration
            if (entity.StartDate.HasValue && entity.EndDate.HasValue)
            {
                entity.DurationInDays = (int)(entity.EndDate.Value - entity.StartDate.Value).TotalDays;
            }

            // 5️⃣ Persist
            var createdEntity = await _repository.AddAsync(entity);

            // 6️⃣ Audit log event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "WBS_CREATE",
                actionName: $"Project WBS '{entity.WorkBreakdownStructureName}' created",
                details: $"WBS created for ProjectId {entity.ProjectId}, Name: {entity.WorkBreakdownStructureName}.",
                module: "ProjectWorkBreakdownStructure"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // 7️⃣ Map to DTO
            var dto = _mapper.Map<ProjectWorkBreakdownStructureDto>(createdEntity);
            dto.ProjectId = project.Id;
            dto.ProjectCode = project.ProjectCode;

            return dto;
        }
    }
}

//        public async Task<ProjectWorkBreakdownStructureDto> Handle(
//         CreateProjectWorkBreakdownStructureCommand request,
//         CancellationToken cancellationToken)
//     {
//         // 1️⃣ Validate ProjectId and get the related ProjectMaster
//         var project = await _repository.GetProjectAsync(request.ProjectId);
//         if (project == null)
//         {
//             throw new ArgumentException("Invalid ProjectId");
//         }

//         // Ensure ProjectCode is not null and is populated for the ProjectMaster
//         if (string.IsNullOrEmpty(project.ProjectCode))
//         {
//             throw new ArgumentException("ProjectCode cannot be null or empty.");
//         }

//         // 2️⃣ Parent WBS validation (if applicable)
//         int level = 1; // Default level is 1 for root-level WBS
//         if (request.ParentWorkBreakdownStructureId.HasValue)
//         {
//             // If Parent is provided, validate the Parent Work Breakdown Structure
//             var parentEntity = await _repository.GetByIdAsync(request.ParentWorkBreakdownStructureId.Value);

//             if (parentEntity == null)
//             {
//                 _logger.LogError($"Parent WBS not found. Provided ParentWorkBreakdownStructureId: {request.ParentWorkBreakdownStructureId.Value}");
//                 throw new ArgumentException("Invalid Parent WBS");
//             }

//             // If the parent exists, set the level
//             level = parentEntity.Level + 1;
//         }

//         // 3️⃣ Map command to entity
//         var entity = _mapper.Map<Core.Domain.Entities.ProjectWorkBreakdownStructure>(request);
//         entity.Level = level;

//         // 4️⃣ Set the ProjectCode from the ProjectMaster to the WBS entity
//     //    entity.ProjectCode = project.ProjectCode; // Access ProjectCode through the Project navigation property

//         // 5️⃣ Duration calculation (if start and end date are provided)
//         if (entity.StartDate.HasValue && entity.EndDate.HasValue)
//         {
//             entity.DurationInDays = (entity.EndDate.Value - entity.StartDate.Value).Days;
//         }

//         // 6️⃣ Save the entity to the database
//         var createdEntity = await _repository.AddAsync(entity);

//         // 7️⃣ Publish Domain Event for auditing
//         var domainEvent = new AuditLogsDomainEvent(
//             actionDetail: "Create",
//             actionCode: "WBS_CREATE",
//             actionName: $"Project Work Breakdown Structure '{entity.WorkBreakdownStructureName}' created",
//             details: $"ProjectWorkBreakdownStructure created for ProjectId {entity.ProjectId} with WBS Name: {entity.WorkBreakdownStructureName}.",
//             module: "ProjectWorkBreakdownStructure"
//         );
//         await _mediator.Publish(domainEvent, cancellationToken);

//         // 8️⃣ Map saved entity to DTO for response
//         var resultDto = _mapper.Map<ProjectWorkBreakdownStructureDto>(createdEntity);

//         return resultDto;
//     }
// }
// }