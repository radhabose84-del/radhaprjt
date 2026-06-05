using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.RawMaterialPO.Commands.CreateRawMaterialPO
{
    public class CreateRawMaterialPOCommandHandler : IRequestHandler<CreateRawMaterialPOCommand, ApiResponseDTO<int>>
    {
        private readonly IRawMaterialPOCommandRepository _commandRepository;
        private readonly IRawMaterialPOQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMiscMasterQueryRepository _misc;
        private readonly IRawMaterialPOFileStorage _fileStorage;

        public CreateRawMaterialPOCommandHandler(
            IRawMaterialPOCommandRepository commandRepository,
            IRawMaterialPOQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IMiscMasterQueryRepository misc,
            IRawMaterialPOFileStorage fileStorage)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _misc = misc;
            _fileStorage = fileStorage;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateRawMaterialPOCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<RawMaterialPOHeader>(request);

            var unitId = _ipAddressService.GetUnitId()
                ?? throw new ExceptionRules("UnitId is not available for the current user.");
            entity.UnitId = unitId;
            entity.PODate = request.PODate == default ? DateTimeOffset.UtcNow : request.PODate;

            // Generate PONumber from DocumentSequence (TransactionType master)
            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeRMPO, MiscEnumEntity.ModulePurchase, unitId)
                ?? throw new ExceptionRules("No transaction type configured for Raw Material Purchase Order.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
            entity.PONumber = sequences.Count > 0
                ? sequences[^1]
                : throw new ExceptionRules("No document sequence configured for Raw Material Purchase Order.");

            // Detail lines + totals are saved exactly as supplied in the payload (no recomputation)
            entity.RawMaterialPODetails = request.Details.Select(MapDetail).ToList();

            // Auto-calculate conversion status from cumulative converted bales vs OCR total
            var ocrQuantity = await _queryRepository.GetOcrQuantityAsync(request.OcrId);
            var alreadyConverted = await _queryRepository.GetConvertedQuantityAsync(request.OcrId, null);
            var totalConverted = alreadyConverted + request.Details.Sum(d => d.Quantity);

            var statusName = totalConverted >= ocrQuantity
                ? MiscEnumEntity.FullyConverted
                : MiscEnumEntity.PartiallyConverted;
            var statusMisc = await _misc.GetMiscMasterByName(MiscEnumEntity.ConversionStatus, statusName)
                ?? throw new ExceptionRules(
                    $"Conversion status '{statusName}' is not configured. Seed the '{MiscEnumEntity.ConversionStatus}' MiscType values.");
            entity.StatusId = statusMisc.Id;

            // Rename the uploaded document (staged under a temp name during upload) to the PO number,
            // then persist. Wrapped in try/catch so a persist failure cleans up the renamed file.
            int newId;
            string? savedFileName = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(entity.DocumentPath))
                {
                    savedFileName = await _fileStorage.RenameAsync(entity.DocumentPath, entity.PONumber, cancellationToken);
                    entity.DocumentPath = savedFileName;
                }

                newId = await _commandRepository.CreateAsync(entity, transactionTypeId, cancellationToken);
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

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "RAWMATERIALPO_CREATE",
                actionName: entity.PONumber,
                details: $"Raw Material PO '{entity.PONumber}' created successfully with Id {newId}.",
                module: "RawMaterialPO");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Raw Material PO created successfully.",
                Data = newId
            };
        }

        private static RawMaterialPODetail MapDetail(CreateRawMaterialPODetailDto d) => new()
        {
            ItemId = d.ItemId,
            HsnId = d.HsnId,
            Quantity = d.Quantity,
            Weight = d.Weight,
            Rate = d.Rate,
            ItemValue = d.ItemValue,
            CGSTPercentage = d.CGSTPercentage,
            SGSTPercentage = d.SGSTPercentage,
            IGSTPercentage = d.IGSTPercentage,
            CGSTValue = d.CGSTValue,
            SGSTValue = d.SGSTValue,
            IGSTValue = d.IGSTValue,
            TotalGST = d.TotalGST,
            NetValue = d.NetValue
        };
    }
}
