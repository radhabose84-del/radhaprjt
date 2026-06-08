using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOFromOcr
{
    /// <summary>
    /// Auto-fetches approved-OCR details for the conversion screen and reports the cumulative
    /// conversion progress (Not / Partially / Fully Converted) and RemainingQuantity ("Max N Bales").
    /// </summary>
    public class GetRawMaterialPOFromOcrQueryHandler : IRequestHandler<GetRawMaterialPOFromOcrQuery, OcrConversionDto?>
    {
        private readonly IOCREntryQueryRepository _ocrQueryRepository;
        private readonly IRawMaterialPOQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetRawMaterialPOFromOcrQueryHandler(
            IOCREntryQueryRepository ocrQueryRepository,
            IRawMaterialPOQueryRepository queryRepository,
            IMediator mediator)
        {
            _ocrQueryRepository = ocrQueryRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<OcrConversionDto?> Handle(GetRawMaterialPOFromOcrQuery request, CancellationToken cancellationToken)
        {
            var ocr = await _ocrQueryRepository.GetByIdAsync(request.OcrId);
            if (ocr == null)
                return null;

            var converted = await _queryRepository.GetConvertedQuantityAsync(request.OcrId, null);
            var remaining = ocr.Quantity - converted;

            var status = converted <= 0
                ? MiscEnumEntity.NotConverted
                : converted < ocr.Quantity
                    ? MiscEnumEntity.PartiallyConverted
                    : MiscEnumEntity.FullyConverted;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetFromOcr",
                actionCode: "GetRawMaterialPOFromOcrQuery",
                actionName: request.OcrId.ToString(),
                details: $"OCR {request.OcrId} fetched for conversion.",
                module: "RawMaterialPO");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new OcrConversionDto
            {
                Ocr = ocr,
                ConvertedQuantity = converted,
                RemainingQuantity = remaining,
                ConversionStatus = status
            };
        }
    }
}
