using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Purchase;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Purchase;
using FluentValidation;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.GateInward.Dto;
using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;
using GateEntryManagement.Domain.Events;
using MediatR;
using System.IO;

namespace GateEntryManagement.Application.GateInward.Commands.CreateGateInward
{
    public class CreateGateInwardCommandHandler : IRequestHandler<CreateGateInwardCommand, ApiResponseDTO<int>>
    {
        private readonly IGateInwardCommandRepository _commandRepository;
        private readonly IGateInwardQueryRepository _queryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly ITransactionTypeLookup _transactionTypeLookup;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly IGateInwardAttachmentFileStorage _attachmentStorage;
        private readonly IGateInwardGrnBridge _grnBridge;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IVehicleMovementRecordCommandRepository _vmrCommandRepository;

        public CreateGateInwardCommandHandler(
            IGateInwardCommandRepository commandRepository,
            IGateInwardQueryRepository queryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            ITransactionTypeLookup transactionTypeLookup,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService,
            IGateInwardAttachmentFileStorage attachmentStorage,
            IGateInwardGrnBridge grnBridge,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IVehicleMovementRecordCommandRepository vmrCommandRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _transactionTypeLookup = transactionTypeLookup;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
            _attachmentStorage = attachmentStorage;
            _grnBridge = grnBridge;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _vmrCommandRepository = vmrCommandRepository;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateGateInwardCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<GateInwardHdr>(request);

            // Auto-calculate Net Weight
            if (request.GrossWeight.HasValue && request.TareWeight.HasValue)
                entity.NetWeight = request.GrossWeight.Value - request.TareWeight.Value;

            // Map detail lines (in memory, not yet persisted)
            if (request.GateInwardDetails != null && request.GateInwardDetails.Count > 0)
            {
                entity.GateInwardDetails = _mapper.Map<List<GateInwardDtl>>(request.GateInwardDetails);
            }

            // ─── Pre-validate GRN BEFORE saving Gate Inward ──────────────────────────────
            // PO-backed lines drive the GRN auto-fire. Non-PO lines (PoId = 0) are gate-only.
            // A line is "PO-backed" only when both PoId AND DcQuantity are present and > 0.
            // Non-PO lines (manual receipts) have null PoId / DcQuantity and are saved on Gate
            // Inward only — no GRN row gets created for them.
            var poBackedLines = entity.GateInwardDetails?
                .Where(d => d.PoId.HasValue && d.PoId.Value > 0
                         && d.DcQuantity.HasValue && d.DcQuantity.Value > 0)
                .ToList();

            if (poBackedLines is { Count: > 0 })
            {
                // Resolve the doc-type ShortName (LPO / IPO / …) from the line's ReferenceDocTypeId.
                // All PO-backed lines must share the same ReferenceDocTypeId per FE flow — we use the first.
                var refDocTypeId = poBackedLines[0].ReferenceDocTypeId;
                var documentTypeCode = await ResolveDocumentTypeCodeAsync(refDocTypeId);
                if (string.IsNullOrWhiteSpace(documentTypeCode))
                    throw new ExceptionRules(
                        $"ReferenceDocumentTypeId={refDocTypeId} could not be resolved to a Finance.TransactionTypeMaster ShortName.");

                var grnContext = BuildGrnContext(entity, gateEntryId: 0, documentTypeCode, poBackedLines);
                var errors = await _grnBridge.ValidateAsync(grnContext, cancellationToken);
                if (errors.Count > 0)
                {
                    throw new ValidationException(string.Join("; ", errors));
                }
            }
            // ─────────────────────────────────────────────────────────────────────────────

            // Auto-generate Gate Entry No via DocumentSequence (only after GRN validation passed)
            var unitId = _ipAddressService.GetUnitId() ?? request.UnitId;
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeGateInward, MiscEnumEntity.ModuleGateEntry, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Gate Inward' not found for Gate Entry module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var gateEntryNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.GateEntryNo = gateEntryNo
                ?? throw new ExceptionRules("No document sequence configured for Gate Inward.");

            // Single optional attachment: move staged file → permanent misc-configured folder
            string? movedRelativePath = null;
            if (request.Attachment != null && !string.IsNullOrWhiteSpace(request.Attachment.FileName))
            {
                var dirs = await _queryRepository.GetDocumentDirectoryPath();
                var subFolder = dirs.GetValueOrDefault(MiscEnumEntity.GateEntryImage, string.Empty);

                movedRelativePath = await _attachmentStorage.MoveStagedToPermanentAsync(
                    request.Attachment.FileName, subFolder, entity.GateEntryNo!, cancellationToken);

                entity.AttachmentFileName = Path.GetFileName(movedRelativePath);
                entity.AttachmentFilePath = movedRelativePath;
            }

            int newId;
            try
            {
                newId = await _commandRepository.CreateAsync(entity, typeId.Value);
            }
            catch
            {
                if (movedRelativePath != null)
                {
                    try { await _attachmentStorage.DeleteAsync(movedRelativePath, CancellationToken.None); }
                    catch { /* best-effort cleanup */ }
                }
                throw;
            }

            // ─── Auto-fire GRN for PO-backed lines ───────────────────────────────────────
            // Atomicity contract: "GRN failed means Gate Inward could not save."
            // Pre-validation already caught tolerance/qty issues, so this path normally succeeds.
            // If the downstream GRN insert still throws (DB outage, FK violation, unexpected state),
            // we soft-delete the just-saved Gate Inward + clean up the moved attachment so the
            // user sees a clean failure with nothing orphaned.
            if (poBackedLines is { Count: > 0 })
            {
                var refDocTypeId = poBackedLines[0].ReferenceDocTypeId;
                var documentTypeCode = await ResolveDocumentTypeCodeAsync(refDocTypeId);
                var grnContext = BuildGrnContext(entity, gateEntryId: newId, documentTypeCode, poBackedLines);

                try
                {
                    await _grnBridge.CreateAsync(grnContext, cancellationToken);
                }
                catch
                {
                    // Compensate — soft-delete the Gate Inward we just persisted.
                    try { await _commandRepository.SoftDeleteAsync(newId, CancellationToken.None); }
                    catch { /* best-effort cleanup */ }

                    // Best-effort attachment cleanup if we moved one earlier.
                    if (movedRelativePath != null)
                    {
                        try { await _attachmentStorage.DeleteAsync(movedRelativePath, CancellationToken.None); }
                        catch { /* best-effort cleanup */ }
                    }

                    throw;
                }
            }
            // ─────────────────────────────────────────────────────────────────────────────

            // ─── Mark the linked VMR as exited (OUT) ─────────────────────────────────────
            // Runs after Gate Inward (and any auto-fired GRN) have succeeded, so a failure
            // upstream leaves the VMR in IN state and the operator can retry. When the Gate
            // Inward has no VMR (manual / courier flows), this is a no-op.
            if (entity.VehicleMovementRecordId is > 0)
            {
                var outStatusId =
                    (await _miscMasterQueryRepository.GetMiscMasterByName(
                        MiscEnumEntity.VMRStatus, MiscEnumEntity.VMRStatusExited))?.Id
                    ?? throw new ExceptionRules(
                        $"VMR status '{MiscEnumEntity.VMRStatusExited}' is not configured in Gate.MiscMaster under type '{MiscEnumEntity.VMRStatus}'.");

                await _vmrCommandRepository.UpdateStatusToExitedAsync(
                    entity.VehicleMovementRecordId.Value, outStatusId, cancellationToken);
            }
            // ─────────────────────────────────────────────────────────────────────────────

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "GATEINWARD_CREATE",
                actionName: entity.GateEntryNo,
                details: $"Gate Inward '{entity.GateEntryNo}' created successfully with Id {newId}.",
                module: "GateInward"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Gate Inward Entry created successfully.",
                Data = newId
            };
        }

        private async Task<string?> ResolveDocumentTypeCodeAsync(int referenceDocTypeId)
        {
            if (referenceDocTypeId <= 0) return null;
            var txnTypes = await _transactionTypeLookup.GetByIdsAsync(new[] { referenceDocTypeId });
            return txnTypes.FirstOrDefault()?.ShortName;
        }

        private static GateInwardGrnContextDto BuildGrnContext(
            GateInwardHdr entity, int gateEntryId, string? documentTypeCode, List<GateInwardDtl> poBackedLines) =>
            new()
            {
                PartyId = entity.PartyId ?? 0,
                UnitId = entity.UnitId,
                GateEntryId = gateEntryId,
                DocumentTypeCode = documentTypeCode,
                InvoiceNo = entity.InvoiceNo,
                InvoiceDate = entity.InvoiceDate ?? DateTimeOffset.UtcNow,
                DcNo = entity.DcNo,
                DcDate = entity.DcDate,
                ReceivingWarehouseId = entity.ReceivingWarehouseId,
                Remarks = entity.Remarks,
                Lines = poBackedLines.Select(d => new GateInwardGrnLineDto
                {
                    // poBackedLines is already filtered to HasValue + > 0 — safe to unwrap.
                    PoId = d.PoId!.Value,
                    PoSlNoLocal = d.PoSlNoLocal,
                    DcQuantity = d.DcQuantity!.Value,
                    ExpiryDate = d.ExpiryDate
                }).ToList()
            };
    }
}
