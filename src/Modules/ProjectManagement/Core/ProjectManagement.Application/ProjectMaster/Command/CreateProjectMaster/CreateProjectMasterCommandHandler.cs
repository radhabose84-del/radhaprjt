using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Common.Interfaces.IOutbox;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster
{
    public class CreateProjectMasterCommandHandler : IRequestHandler<CreateProjectMasterCommand, ProjectMasterDto>
    {
        private readonly IProjectMasterCommandRepository _commandRepo;
        private readonly ILogger<CreateProjectMasterCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ip;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;

        public CreateProjectMasterCommandHandler(
            IProjectMasterCommandRepository commandRepo,
            IMapper mapper,
            ILogger<CreateProjectMasterCommandHandler> logger,
            IMediator mediator,
            IIPAddressService ip,
            ITimeZoneService timeZoneService,
            IOutboxEventPublisher outboxEventPublisher)
        {
            _commandRepo = commandRepo;
            _mapper = mapper;
            _logger = logger;
            _mediator = mediator;
            _ip = ip;
            _timeZoneService = timeZoneService;
            _outboxEventPublisher = outboxEventPublisher;
        }

        public async Task<ProjectMasterDto> Handle(CreateProjectMasterCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Project;

            // 1) Map DTO → Entity
            var entity = _mapper.Map<ProjectManagement.Domain.Entities.ProjectMaster>(dto);

            // 2) Persist entity
            entity = await _commandRepo.CreateAsync(entity, cancellationToken);

            // 3) Handle documents if provided
            if (dto.Documents != null && dto.Documents.Any())
            {
                dto.Documents = dto.Documents
                    .Where(d =>
                        d.DocumentId != 0 &&
                        !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (dto.Documents.Any())
                {
                    var baseDirectory = MiscEnumEntity.DocumentPath;
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
                    EnsureDirectoryExists(uploadPath);

                    foreach (var doc in dto.Documents)
                    {
                        if (string.IsNullOrWhiteSpace(doc.FileName))
                            continue;

                        var oldFilePath = Path.Combine(uploadPath, doc.FileName);
                        if (!File.Exists(oldFilePath))
                            continue;

                        var extension = Path.GetExtension(oldFilePath);
                        var newFileName = $"{entity.ProjectCode}_{doc.DocumentId}{extension}";
                        var newFilePath = Path.Combine(uploadPath, newFileName);

                        try
                        {
                            File.Move(oldFilePath, newFilePath, overwrite: true);
                            doc.FileName = newFileName;

                            if (doc.UploadedDate == default)
                                doc.UploadedDate = DateTimeOffset.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Failed renaming project document to {NewFileName} for ProjectId {ProjectId}",
                                newFileName, entity.Id);
                        }
                    }

                    entity.ProjectDocuments = dto.Documents.Select(d => new ProjectDocument
                    {
                        ProjectId = entity.Id,
                        DocumentId = d.DocumentId,
                        FileName = d.FileName,
                        UploadedDate = d.UploadedDate
                    }).ToList();

                    await _commandRepo.UpdateAsync(entity, cancellationToken);
                }
            }

            // 4) Audit log
            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "CreateProject",
                actionName: "ProjectMaster",
                details: JsonSerializer.Serialize(new { ProjectId = entity.Id, ProjectCode = entity.ProjectCode }),
                module: "Project"), cancellationToken);

            // 5) Reload full aggregate for workflow payload
            var agg = await _commandRepo.GetByIdAsync(entity.Id, cancellationToken)
                      ?? throw new InvalidOperationException($"ProjectMaster {entity.Id} not found after create.");

            // 6) Build workflow payload and schedule outbox event
            var header = _mapper.Map<ProjectMasterWorkFlowDto>(agg);
            var wf = new CreateProjectMasterWorkflowDto
            {
                Header = header,
                Lines = new List<ProjectMasterWorkFlowDto>()
            };

            var payload = JsonSerializer.Serialize(wf);
            var correlationId = Guid.NewGuid();

            var @event = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.ProjectMaster,
                ModuleTransactionId = entity.Id,
                Payload = payload
            };

            await _outboxEventPublisher.ScheduleAsync(@event, correlationId, cancellationToken);

            _logger.LogInformation(
                "ProjectMaster created. Id={ProjectId}, Code={ProjectCode}, CorrelationId={CorrelationId}",
                entity.Id, entity.ProjectCode, correlationId);

            // 7) Map back to DTO
            var result = _mapper.Map<ProjectMasterDto>(entity);
            return result;
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
