using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Commands.UpdateOCREntry
{
    public class UpdateOCREntryCommandHandler : IRequestHandler<UpdateOCREntryCommand, ApiResponseDTO<int>>
    {
        private readonly IOCREntryCommandRepository _commandRepository;
        private readonly IOCREntryQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IOCREntryFileStorage _fileStorage;

        public UpdateOCREntryCommandHandler(
            IOCREntryCommandRepository commandRepository,
            IOCREntryQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            IOCREntryFileStorage fileStorage)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _fileStorage = fileStorage;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateOCREntryCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.OCREntry>(request);

            // A freshly uploaded document arrives under a temp name — rename it to the OCR number.
            if (!string.IsNullOrWhiteSpace(entity.DocumentPath) &&
                entity.DocumentPath.StartsWith("TEMP_", StringComparison.OrdinalIgnoreCase))
            {
                var workflow = await _commandRepository.GetByIdOCRWorkFlowAsync(request.Id);
                entity.DocumentPath = await _fileStorage.RenameAsync(
                    entity.DocumentPath, workflow.OcrNumber!, cancellationToken);
            }

            var result = await _commandRepository.UpdateAsync(entity, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "OCR_UPDATE",
                actionName: request.Id.ToString(),
                details: $"OCR with Id {request.Id} updated successfully.",
                module: "OCREntry");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "OCR updated successfully.",
                Data = result
            };
        }
    }
}
