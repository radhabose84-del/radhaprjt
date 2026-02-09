// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
// using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
// using ProjectManagement.Domain.Entities;
// using MediatR;
// using Microsoft.EntityFrameworkCore;


// namespace ProjectManagement.Application.ProjectMaster.Command.UpdateProjectMaster
// {
//     public class UpdateProjectMasterCommandHandler : IRequestHandler<UpdateProjectMasterCommand, ProjectMasterDto>
//     {
//         private readonly IProjectMasterCommandRepository _iProjectMasterCommandRepository;
//         private readonly IProjectMasterQueryRepository _projectMasterQueryRepository;
//         private readonly IMapper _mapper;

//         public UpdateProjectMasterCommandHandler(
//             IProjectMasterCommandRepository iProjectMasterCommandRepository,
//             IProjectMasterQueryRepository projectMasterQueryRepository,
//             IMapper mapper)
//         {
//             _iProjectMasterCommandRepository = iProjectMasterCommandRepository;
//             _projectMasterQueryRepository = projectMasterQueryRepository;
//             _mapper = mapper;
//         }

//         public async Task<ProjectMasterDto> Handle(UpdateProjectMasterCommand request, CancellationToken cancellationToken)
//         {
//             var dto = request.Project ?? throw new Exception("Payload is required.");

//             // Fetch existing
//             var entity = await _iProjectMasterCommandRepository.GetByIdAsync(dto.Id, cancellationToken);
//             if (entity == null)
//                 throw new Exception($"ProjectMaster with Id {dto.Id} not found.");

//             // Map basic fields
//             _mapper.Map(dto, entity);

//             // Handle documents
//             HandleDocuments(entity, dto);

//             // Save changes
//             await _iProjectMasterCommandRepository.UpdateAsync(entity, cancellationToken);

//             // Reload full DTO from Dapper
//             var updated = await _projectMasterQueryRepository.GetByIdAsync(entity.Id, cancellationToken);
//             if (updated == null)
//                 throw new Exception("Failed to reload updated project.");

//             return _mapper.Map<ProjectMasterDto>(updated);
//         }

//         private void HandleDocuments(ProjectManagement.Domain.Entities.ProjectMaster entity, UpdateProjectMasterDto dto)
//         {
//             entity.ProjectDocuments ??= new List<ProjectDocument>();

//             // Use LIVE collection
//             var existingDocs = entity.ProjectDocuments;

//             var incomingDocs = dto.Documents
//                 .Where(d => d.DocumentId != 0 &&
//                             !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
//                 .ToList();

//             // 1. Remove missing docs
//             var toRemove = existingDocs
//                 .Where(ed => !incomingDocs.Any(d =>
//                     d.DocumentId == ed.DocumentId &&
//                     string.Equals(d.FileName, ed.FileName, StringComparison.OrdinalIgnoreCase)))
//                 .ToList();

//             foreach (var rm in toRemove)
//                 entity.ProjectDocuments.Remove(rm);

//             // 2. Add or update docs
//             foreach (var docDto in incomingDocs)
//             {
//                 var existing = existingDocs.FirstOrDefault(d =>
//                     d.DocumentId == docDto.DocumentId &&
//                     string.Equals(d.FileName, docDto.FileName, StringComparison.OrdinalIgnoreCase));

//                 if (existing == null)
//                 {
//                     entity.ProjectDocuments.Add(new ProjectDocument
//                     {
//                         ProjectId = entity.Id,
//                         DocumentId = docDto.DocumentId,
//                         FileName = docDto.FileName,
//                         UploadedDate = docDto.UploadedDate == default
//                             ? DateTimeOffset.UtcNow
//                             : docDto.UploadedDate
//                     });
//                 }
//                 else
//                 {
//                     if (docDto.UploadedDate != default)
//                         existing.UploadedDate = docDto.UploadedDate;
//                 }
//             }
//         }
//     }
// }

// //        public async Task<ProjectMasterDto> Handle(UpdateProjectMasterCommand request, CancellationToken cancellationToken)
// //         {
// //             var dto = request.Project ?? throw new Exception("Payload is required.");

// //             // Fetch existing ProjectMaster entity
// //             var entity = await _iProjectMasterCommandRepository.GetByIdAsync(dto.Id, cancellationToken);
// //             if (entity == null)
// //                 throw new Exception($"ProjectMaster with Id {dto.Id} not found.");

// //             // Map scalar fields from DTO to entity
// //             _mapper.Map(dto, entity);

// //             // Handle document updates: Add new, Remove obsolete, or update existing ones
// //             HandleDocuments(entity, dto);

// //             // Persist the changes to the database
// //             await _iProjectMasterCommandRepository.UpdateAsync(entity, cancellationToken);

// //             // Reload documents after update (ensure documents are included)
// //             var projectMasterWithDocuments = await _projectMasterQueryRepository.GetByIdAsync(entity.Id, cancellationToken);
// //             entity.ProjectDocuments = projectMasterWithDocuments?.ProjectDocuments ?? new List<ProjectDocument>();

// //             // Return updated ProjectMaster as DTO, including Documents
// //             var result = _mapper.Map<ProjectMasterDto>(entity);
// //             return result;
// //         }

// //         private void HandleDocuments(ProjectManagement.Domain.Entities.ProjectMaster entity, UpdateProjectMasterDto dto)
// //         {
// //             entity.ProjectDocuments ??= new List<ProjectDocument>();

// //             var existingDocs = entity.ProjectDocuments.ToList();
// //             var validIncomingDocs = dto.Documents
// //                 .Where(d => d.DocumentId != 0 && !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
// //                 .ToList();

// //             // 1. Remove documents that are no longer in the payload
// //             var toRemove = existingDocs
// //                 .Where(ed => !validIncomingDocs.Any(d =>
// //                     d.DocumentId == ed.DocumentId &&
// //                     string.Equals(d.FileName, ed.FileName, StringComparison.OrdinalIgnoreCase)))  // Ensure no extra spaces or mismatched filenames
// //                 .ToList();

// //             foreach (var rm in toRemove)
// //             {
// //                 entity.ProjectDocuments.Remove(rm);  // Remove document from the collection
// //             }

// //             // 2. Add new documents (those with Id == 0 in the DTO)
// //             foreach (var docDto in validIncomingDocs)
// //             {
// //                 var existingDoc = existingDocs.FirstOrDefault(d =>
// //                     d.DocumentId == docDto.DocumentId &&
// //                     string.Equals(d.FileName, docDto.FileName, StringComparison.OrdinalIgnoreCase));

// //                 if (existingDoc == null)
// //                 {
// //                     // Add new document if not already existing
// //                     var newDoc = new ProjectDocument
// //                     {
// //                         ProjectId = entity.Id,
// //                         DocumentId = docDto.DocumentId,
// //                         FileName = docDto.FileName,
// //                         UploadedDate = docDto.UploadedDate == default ? DateTimeOffset.UtcNow : docDto.UploadedDate
// //                     };

// //                     entity.ProjectDocuments.Add(newDoc);  // Add the new document to the collection
// //                 }
// //                 else
// //                 {
// //                     existingDoc.UploadedDate = docDto.UploadedDate;  // Update the date if necessary
// //                 }
// //             }
// //         }
// //     }
// // }