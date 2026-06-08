using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.RawMaterialPO.Commands.UpdateRawMaterialPO
{
    public class UpdateRawMaterialPOCommandHandler : IRequestHandler<UpdateRawMaterialPOCommand, ApiResponseDTO<int>>
    {
        private readonly IRawMaterialPOCommandRepository _commandRepository;
        private readonly IRawMaterialPOQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IMiscMasterQueryRepository _misc;
        private readonly IRawMaterialPOFileStorage _fileStorage;

        public UpdateRawMaterialPOCommandHandler(
            IRawMaterialPOCommandRepository commandRepository,
            IRawMaterialPOQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            IMiscMasterQueryRepository misc,
            IRawMaterialPOFileStorage fileStorage)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _misc = misc;
            _fileStorage = fileStorage;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateRawMaterialPOCommand request, CancellationToken cancellationToken)
        {
            var existing = await _queryRepository.GetByIdAsync(request.Id)
                ?? throw new ExceptionRules("Raw Material PO not found.");

            var entity = _mapper.Map<RawMaterialPOHeader>(request);

            // A freshly uploaded document arrives under a temp name — rename it to the PO number.
            if (!string.IsNullOrWhiteSpace(entity.DocumentPath) &&
                entity.DocumentPath.StartsWith("TEMP_", StringComparison.OrdinalIgnoreCase))
            {
                entity.DocumentPath = await _fileStorage.RenameAsync(
                    entity.DocumentPath, existing.PONumber!, cancellationToken);
            }

            // Detail lines + totals saved exactly as supplied (PONumber/OcrId are immutable)
            entity.RawMaterialPODetails = request.Details.Select(MapDetail).ToList();

            // Re-evaluate conversion status against the OCR balance (excluding this header)
            var ocrQuantity = await _queryRepository.GetOcrQuantityAsync(existing.OcrId);
            var otherConverted = await _queryRepository.GetConvertedQuantityAsync(existing.OcrId, request.Id);
            var totalConverted = otherConverted + request.Details.Sum(d => d.Quantity);

            var statusName = totalConverted >= ocrQuantity
                ? MiscEnumEntity.FullyConverted
                : MiscEnumEntity.PartiallyConverted;
            var statusMisc = await _misc.GetMiscMasterByName(MiscEnumEntity.ConversionStatus, statusName)
                ?? throw new ExceptionRules(
                    $"Conversion status '{statusName}' is not configured. Seed the '{MiscEnumEntity.ConversionStatus}' MiscType values.");
            entity.StatusId = statusMisc.Id;

            var result = await _commandRepository.UpdateAsync(entity, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "RAWMATERIALPO_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Raw Material PO with Id {request.Id} updated successfully.",
                module: "RawMaterialPO");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Raw Material PO updated successfully.",
                Data = result
            };
        }

        private static RawMaterialPODetail MapDetail(UpdateRawMaterialPODetailDto d) => new()
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
