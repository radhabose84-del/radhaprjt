using AutoMapper;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using ProjectManagement.Domain.Entities;
using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Command.UpdateProjectMaster
{
    public class UpdateProjectMasterCommandHandler : IRequestHandler<UpdateProjectMasterCommand, ProjectMasterDto>
    {
        private readonly IProjectMasterCommandRepository _iProjectMasterCommandRepository;
        private readonly IProjectMasterQueryRepository _projectMasterQueryRepository;
        private readonly IMapper _mapper;

        public UpdateProjectMasterCommandHandler(
            IProjectMasterCommandRepository iProjectMasterCommandRepository,
            IProjectMasterQueryRepository projectMasterQueryRepository,
            IMapper mapper)
        {
            _iProjectMasterCommandRepository = iProjectMasterCommandRepository;
            _projectMasterQueryRepository = projectMasterQueryRepository;
            _mapper = mapper;
        }

        public async Task<ProjectMasterDto> Handle(UpdateProjectMasterCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Project ?? throw new Exception("Payload is required.");

            // Fetch existing
            var entity = await _iProjectMasterCommandRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (entity == null)
                throw new Exception($"ProjectMaster with Id {dto.Id} not found.");

            // Map basic fields
            _mapper.Map(dto, entity);

            // Handle documents
            HandleDocuments(entity, dto);

            // Save changes
            await _iProjectMasterCommandRepository.UpdateAsync(entity, cancellationToken);

            // Reload full DTO from Dapper
            var updated = await _projectMasterQueryRepository.GetByIdAsync(entity.Id, cancellationToken);
            if (updated == null)
                throw new Exception("Failed to reload updated project.");

            return _mapper.Map<ProjectMasterDto>(updated);
        }

        private void HandleDocuments(ProjectManagement.Domain.Entities.ProjectMaster entity, UpdateProjectMasterDto dto)
        {
            entity.ProjectDocuments ??= new List<ProjectDocument>();

            var existingDocs = entity.ProjectDocuments;

            var incomingDocs = dto.Documents
                .Where(d => d.DocumentId != 0 &&
                            !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 1. Remove missing docs
            var toRemove = existingDocs
                .Where(ed => !incomingDocs.Any(d =>
                    d.DocumentId == ed.DocumentId &&
                    string.Equals(d.FileName, ed.FileName, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var rm in toRemove)
                entity.ProjectDocuments.Remove(rm);

            // 2. Add or update docs
            foreach (var docDto in incomingDocs)
            {
                var existing = existingDocs.FirstOrDefault(d =>
                    d.DocumentId == docDto.DocumentId &&
                    string.Equals(d.FileName, docDto.FileName, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    entity.ProjectDocuments.Add(new ProjectDocument
                    {
                        ProjectId = entity.Id,
                        DocumentId = docDto.DocumentId,
                        FileName = docDto.FileName,
                        UploadedDate = docDto.UploadedDate == default
                            ? DateTimeOffset.UtcNow
                            : docDto.UploadedDate
                    });
                }
                else
                {
                    if (docDto.UploadedDate != default)
                        existing.UploadedDate = docDto.UploadedDate;
                }
            }
        }
    }
}
