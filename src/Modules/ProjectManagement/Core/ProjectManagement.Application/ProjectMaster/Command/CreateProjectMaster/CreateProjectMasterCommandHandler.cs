// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Events.Workflow;
// using Contracts.Interfaces.External.IWorkflow;
// using Contracts.Interfaces;
using ProjectManagement.Application.Common.Interfaces;
// using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
// using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
// using ProjectManagement.Domain.Common;
// using ProjectManagement.Domain.Entities;
// using ProjectManagement.Domain.Events;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster
// {
//     public class CreateProjectMasterCommandHandler : IRequestHandler<CreateProjectMasterCommand, ProjectMasterDto>
//     {

//         private readonly IProjectMasterCommandRepository _commandRepo;        
//         private readonly ILogger<CreateProjectMasterCommandHandler> _logger;             
//          private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IIPAddressService _ip;
//         private readonly ITimeZoneService _timeZoneService;        
//         private readonly IEventPublisher _eventPublisher;




//         public CreateProjectMasterCommandHandler(IProjectMasterCommandRepository commandRepo, IMapper mapper, ILogger<CreateProjectMasterCommandHandler> logger, IMediator mediator, IIPAddressService ip, ITimeZoneService timeZoneService, IEventPublisher eventPublisher)
//         {
//             _commandRepo = commandRepo;
//             _mapper = mapper;
//             _logger = logger;
//             _mediator = mediator;
//             _ip = ip;
//             _timeZoneService = timeZoneService;
//             _eventPublisher = eventPublisher;
//         }
       

//         public async Task<ProjectMasterDto> Handle( CreateProjectMasterCommand request,  CancellationToken cancellationToken)
//         {
//             var dto = request.Project;

//             // 1️⃣ Map DTO → Entity
//             var entity = _mapper.Map< ProjectManagement.Domain.Entities.ProjectMaster>(dto);

         
//             // 3️⃣ Persist entity first (so we have Id, timestamps etc.)
//             entity = await _commandRepo.CreateAsync(entity, cancellationToken);

//             // 4️⃣ Handle documents if provided
//             if (dto.Documents != null && dto.Documents.Any())
//             {
//                 // Keep only real docs (skip placeholders from UI)
//                 dto.Documents = dto.Documents
//                     .Where(d =>
//                         d.DocumentId != 0 &&
//                         !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
//                     .ToList();

//                 if (dto.Documents.Any())
//                 {
//                     // Base directory – same as UploadDocument handler
//                     var baseDirectory = MiscEnumEntity.DocumentPath; // or MiscEnumEntity.ProjectDocumentPath
//                     var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
//                     EnsureDirectoryExists(uploadPath);

//                     foreach (var doc in dto.Documents)
//                     {
//                         if (string.IsNullOrWhiteSpace(doc.FileName))
//                             continue;

//                         var oldFilePath = Path.Combine(uploadPath, doc.FileName);
//                         if (!File.Exists(oldFilePath))
//                             continue;

//                         // 🔹 New file name: ProjectCode_DocumentId.ext
//                         var extension = Path.GetExtension(oldFilePath);
//                         var newFileName = $"{entity.ProjectCode}_{doc.DocumentId}{extension}";
//                         var newFilePath = Path.Combine(uploadPath, newFileName);

//                         try
//                         {
//                             File.Move(oldFilePath, newFilePath, overwrite: true);
//                             doc.FileName = newFileName;

//                             if (doc.UploadedDate == default)
//                                 doc.UploadedDate = DateTimeOffset.UtcNow;
//                         }
//                         catch (Exception ex)
//                         {
//                             _logger.LogError(ex,
//                                 "Failed renaming project document to {NewFileName} for ProjectId {ProjectId}",
//                                 newFileName, entity.Id);
//                         }
//                     }

//                     // 🔹 Attach to entity so EF saves ProjectDocument rows
//                     entity.ProjectDocuments = dto.Documents.Select(d => new ProjectDocument
//                     {
//                         ProjectId = entity.Id,       // optional; EF will set if configured
//                         DocumentId = d.DocumentId,
//                         FileName = d.FileName,
//                         UploadedDate = d.UploadedDate
//                     }).ToList();

//                     // Save again to persist ProjectDocuments
//                     await _commandRepo.UpdateAsync(entity, cancellationToken);
//                 }
//             }

//         // 4️⃣ (Optional) Audit log – similar to SES Create
//             await _mediator.Publish(new AuditLogsDomainEvent(
//                 actionDetail: "Create",
//                 actionCode: "CreateProject",
//                 actionName: "ProjectMaster",
//                 details: JsonSerializer.Serialize(new { ProjectId = entity.Id, ProjectCode = entity.ProjectCode }),
//                 module: "Project"), cancellationToken);

//             // 5️⃣ Reload full aggregate if you need navigation props for workflow payload
//             //    If your mapping only needs simple fields, you can skip reload and use `entity` directly.
//             var agg = await _commandRepo.GetByIdAsync(entity.Id, cancellationToken)
//                       ?? throw new InvalidOperationException($"ProjectMaster {entity.Id} not found after create.");

//             // 6️⃣ Build workflow payload – SAME PATTERN as SES
//             //    Ensure you have this DTO + AutoMapper profile:
//             //    CreateMap<ProjectMaster, CreateProjectMasterWorkflowDto>();
//             var header = _mapper.Map<ProjectMasterWorkFlowDto>(agg);
//             var wf = new CreateProjectMasterWorkflowDto
//                 {
//                     Header = header,
//                     Lines = new List<ProjectMasterWorkFlowDto>() // later you can add WBS lines here
//                 };

//                 var payload = JsonSerializer.Serialize(wf);
//            // var payload = JsonSerializer.Serialize(wf); // PascalCase by default
//             var payloadBytes = Encoding.UTF8.GetByteCount(payload);

//             var correlationId = Guid.NewGuid();

//             var evt = new TransactionCreatedEvent
//             {
//                 CorrelationId = correlationId,
//                 // 👇 Use the appropriate enum/string you use for ProjectMaster in your workflow
//                 ModuleTypeName = MiscEnumEntity.ProjectMaster,  
//                 ModuleTransactionId = entity.Id,
//                 Payload = payload
//             };

//             // 7️⃣ Outbox save + publish (non-blocking) – EXACTLY LIKE SES
//             try
//             {
//                 await _eventPublisher.SaveEventAsync(evt);
//                 await _eventPublisher.PublishPendingEventsAsync();

//                 _logger.LogInformation(
//                     "ProjectMaster created. Id={ProjectId}, Code={ProjectCode}, CorrelationId={CorrelationId}, PayloadBytes={Bytes}",
//                     entity.Id, entity.ProjectCode, correlationId, payloadBytes);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogWarning(
//                     ex,
//                     "ProjectMaster created but workflow publish deferred/failed. Id={ProjectId}, Code={ProjectCode}, CorrelationId={CorrelationId}",
//                     entity.Id, entity.ProjectCode, correlationId);
//             }
            


//             // 5️⃣ Map back to DTO
//             var result = _mapper.Map<ProjectMasterDto>(entity);
//             return result;
//         }

//         private void EnsureDirectoryExists(string path)
//         {
//             if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
//                 Directory.CreateDirectory(path);
//         }
       

//     }
// }