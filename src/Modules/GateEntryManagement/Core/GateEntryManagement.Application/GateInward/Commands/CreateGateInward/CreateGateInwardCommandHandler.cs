using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
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
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly IGateInwardAttachmentFileStorage _attachmentStorage;

        public CreateGateInwardCommandHandler(
            IGateInwardCommandRepository commandRepository,
            IGateInwardQueryRepository queryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService,
            IGateInwardAttachmentFileStorage attachmentStorage)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
            _attachmentStorage = attachmentStorage;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateGateInwardCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<GateInwardHdr>(request);

            // Auto-calculate Net Weight
            if (request.GrossWeight.HasValue && request.TareWeight.HasValue)
                entity.NetWeight = request.GrossWeight.Value - request.TareWeight.Value;

            // Auto-generate Gate Entry No via DocumentSequence
            var unitId = _ipAddressService.GetUnitId() ?? request.UnitId;
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeGateInward, MiscEnumEntity.ModuleGateEntry, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Gate Inward' not found for Gate Entry module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var gateEntryNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.GateEntryNo = gateEntryNo
                ?? throw new ExceptionRules("No document sequence configured for Gate Inward.");

            // Map detail lines
            if (request.GateInwardDetails != null && request.GateInwardDetails.Count > 0)
            {
                entity.GateInwardDetails = _mapper.Map<List<GateInwardDtl>>(request.GateInwardDetails);
            }

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
    }
}
