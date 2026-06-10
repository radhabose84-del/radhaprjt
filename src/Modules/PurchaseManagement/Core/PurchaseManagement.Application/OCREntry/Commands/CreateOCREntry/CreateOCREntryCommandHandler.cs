using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Commands.CreateOCREntry
{
    public class CreateOCREntryCommandHandler : IRequestHandler<CreateOCREntryCommand, ApiResponseDTO<int>>
    {
        private readonly IOCREntryCommandRepository _commandRepository;
        private readonly IOCREntryQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMiscMasterQueryRepository _misc;
        private readonly IOCREntryFileStorage _fileStorage;

        public CreateOCREntryCommandHandler(
            IOCREntryCommandRepository commandRepository,
            IOCREntryQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher,
            IMiscMasterQueryRepository misc,
            IOCREntryFileStorage fileStorage)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
            _misc = misc;
            _fileStorage = fileStorage;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateOCREntryCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.OCREntry>(request);

            // Build dynamic cotton-quality parameter rows (only when a template is selected).
            if (request.QualityTemplateId.HasValue && request.QualityParameters is { Count: > 0 })
            {
                entity.OcrQualityParameters = request.QualityParameters
                    .Select(p =>
                    {
                        var child = _mapper.Map<Domain.Entities.OCRQualityParameter>(p);
                        child.QualityTemplateId = request.QualityTemplateId.Value;
                        return child;
                    })
                    .ToList();
            }

            // Initial status = Pending Approval (workflow-driven)
            var pendingStatus = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
            entity.StatusId = pendingStatus.Id;

            var unitId = _ipAddressService.GetUnitId()
                ?? throw new ExceptionRules("UnitId is not available for the current user.");

            // Generate OcrNumber from DocumentSequence (TransactionType lookup)
            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeOCR, MiscEnumEntity.ModulePurchase, unitId)
                ?? throw new InvalidOperationException("No transaction type configured for OCR.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
            entity.OcrNumber = sequences.Count > 0
                ? sequences[^1]
                : throw new InvalidOperationException("No document sequence configured for OCR.");

            // Rename the uploaded document (saved under a temp name during upload) to the OCR number,
            // then persist. Wrapped in try/catch so a persist failure cleans up the renamed file.
            Domain.Entities.OCREntry created;
            string? savedFileName = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(entity.DocumentPath))
                {
                    savedFileName = await _fileStorage.RenameAsync(entity.DocumentPath, entity.OcrNumber, cancellationToken);
                    entity.DocumentPath = savedFileName;
                }

                // Persist (IncrementDocNoAsync runs inside the repo transaction)
                created = await _commandRepository.CreateAsync(entity, transactionTypeId, cancellationToken);
            }
            catch
            {
                if (savedFileName != null)
                {
                    try { await _fileStorage.DeleteAsync(savedFileName, CancellationToken.None); }
                    catch { /* best-effort cleanup */ }
                }
                throw;
            }

            // ── Approval request via outbox ──
            var correlationId = Guid.NewGuid();
            var workFlowEntity = await _commandRepository.GetByIdOCRWorkFlowAsync(created.Id);
            workFlowEntity.UnitId = unitId;

            // sp_EvaluateApproval reads the payload via $.Header.* (e.g. $.Header.UnitId) and $.Lines.
            // The payload MUST be shaped as { Header: {...}, Lines: ... } — serializing the DTO flat
            // makes $.Header.UnitId resolve to NULL and the ApprovalRequest insert fails (UnitId NOT NULL).
            var reversePayload = new OCREntryReverseDto
            {
                Header = workFlowEntity,
                Lines = null
            };

            var workflowCommand = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.TransactionTypeOCR,
                ModuleTransactionId = created.Id,
                Payload = JsonSerializer.Serialize(reversePayload)
            };
            await _outboxEventPublisher.ScheduleAsync(workflowCommand, correlationId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "OCR_CREATE",
                actionName: entity.OcrNumber,
                details: $"OCR '{entity.OcrNumber}' created successfully with Id {created.Id}.",
                module: "OCREntry");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "OCR created successfully.",
                Data = created.Id
            };
        }
    }
}
