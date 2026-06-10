using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Purchase;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Dto;
using QCManagement.Domain.Entities;
using QCManagement.Domain.Events;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Application.QcInspection.Commands.CreateQcInspection
{
    public class CreateQcInspectionCommandHandler : IRequestHandler<CreateQcInspectionCommand, ApiResponseDTO<QcInspectionDto>>
    {
        private readonly IQcInspectionCommandRepository _commandRepository;
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IGrnLookup _grnLookup;
        private readonly IArrivalLookup _arrivalLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public CreateQcInspectionCommandHandler(
            IQcInspectionCommandRepository commandRepository,
            IQcInspectionQueryRepository queryRepository,
            IGrnLookup grnLookup,
            IArrivalLookup arrivalLookup,
            IItemLookup itemLookup,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _grnLookup = grnLookup;
            _arrivalLookup = arrivalLookup;
            _itemLookup = itemLookup;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<QcInspectionDto>> Handle(CreateQcInspectionCommand request, CancellationToken cancellationToken)
        {
            // Resolve the source line (GRN or Arrival) into a common shape.
            var arrivalTypeId = await _queryRepository.GetSourceTypeIdByCodeAsync("ARRIVAL");
            var isArrival = arrivalTypeId.HasValue && request.SourceTypeId == arrivalTypeId.Value;

            int sourceHeaderId, sourceItemId, sourceUomId;
            decimal sourceReceivedQty;
            string? sourceBatchNumber;

            if (isArrival)
            {
                var arrival = await _arrivalLookup.GetByArrivalDetailIdAsync(request.SourceDetailId, cancellationToken)
                    ?? throw new ExceptionRules("Arrival line item not found.");
                sourceHeaderId = arrival.ArrivalHeaderId;
                sourceItemId = arrival.ItemId;
                sourceReceivedQty = arrival.ReceivedQuantity;
                sourceUomId = arrival.ReceivedUomId ?? 0;
                sourceBatchNumber = arrival.BatchNumber;
            }
            else
            {
                var grn = await _grnLookup.GetByGrnDetailIdAsync(request.SourceDetailId, cancellationToken)
                    ?? throw new ExceptionRules("GRN line item not found.");
                sourceHeaderId = grn.GrnHeaderId;
                sourceItemId = grn.ItemId;
                sourceReceivedQty = grn.ReceivedQuantity;
                sourceUomId = grn.ReceivedUomId ?? 0;
                sourceBatchNumber = grn.BatchNumber;
            }

            var items = await _itemLookup.GetByIdsAsync(new[] { sourceItemId }, cancellationToken);
            var item = items.FirstOrDefault();
            int? itemCategoryId = item?.ItemCategoryId;

            var qcTypeId = await _queryRepository.GetPurchasedGoodsQcTypeIdAsync()
                ?? throw new ExceptionRules("Purchased-goods QC Type is not configured.");

            var asOf = DateTimeOffset.UtcNow;

            var specId = await _queryRepository.ResolveActiveSpecIdAsync(sourceItemId, itemCategoryId, qcTypeId, asOf)
                ?? throw new ExceptionRules($"No active Quality Specification found for the item as of {asOf:yyyy-MM-dd}.");

            var snapshot = await _queryRepository.GetSpecSnapshotAsync(specId)
                ?? throw new ExceptionRules("Quality Specification snapshot could not be loaded.");

            var year = asOf.Year;
            var seq = await _queryRepository.GetMaxInspectionSequenceAsync(year);
            var inspectionNo = $"QCI-{year}-{(seq + 1):D5}";

            var entity = new QcInspectionHdr
            {
                QcInspectionNo = inspectionNo,
                InspectionDate = asOf,
                SourceTypeId = request.SourceTypeId,
                SourceHeaderId = sourceHeaderId,
                SourceDetailId = request.SourceDetailId,
                QualitySpecificationId = snapshot.QualitySpecificationId,
                QualitySpecificationCode = snapshot.QualitySpecificationCode,
                QualityTemplateId = snapshot.QualityTemplateId,
                QualityTemplateCode = snapshot.QualityTemplateCode,
                QcTypeId = snapshot.QcTypeId,
                InspectorUserId = _ipAddressService.GetUserId(),
                InspectorName = _ipAddressService.GetUserName(),
                ReceivedQuantity = sourceReceivedQty,
                ReceivedUomId = sourceUomId,
                BatchNumber = sourceBatchNumber,
                LotNumber = null,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = snapshot.Parameters.Select(p => new QcInspectionDtl
                {
                    QualitySpecificationParameterId = p.QualitySpecificationParameterId,
                    QualityParameterId = p.QualityParameterId,
                    ParameterCode = p.ParameterCode,
                    ParameterName = p.ParameterName,
                    DataTypeId = p.DataTypeId,
                    ValidationTypeId = p.ValidationTypeId,
                    ValidationTypeCode = p.ValidationTypeCode,
                    UomId = p.UomId,
                    UomCode = p.UomCode,
                    MinValue = p.MinValue,
                    MaxValue = p.MaxValue,
                    ExpectedValue = p.ExpectedValue,
                    AllowedValues = p.AllowedValues,
                    SeverityId = p.SeverityId,
                    SeverityCode = p.SeverityCode,
                    FailureActionId = p.FailureActionId,
                    SortOrder = p.SortOrder,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList()
            };

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "QC_INSPECTION_CREATE",
                actionName: inspectionNo,
                details: $"QC Inspection '{inspectionNo}' created with {entity.Details.Count} parameter(s).",
                module: "QcInspection"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            // Return the full inspection (header + snapshotted parameters) so the UI opens in one call.
            var created = await _queryRepository.GetByIdAsync(newId);

            return new ApiResponseDTO<QcInspectionDto>
            {
                IsSuccess = true,
                Message = "QC Inspection created successfully.",
                Data = created
            };
        }
    }
}
