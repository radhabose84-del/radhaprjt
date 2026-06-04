using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCREntryById
{
    public class GetOCREntryByIdQueryHandler : IRequestHandler<GetOCREntryByIdQuery, OCREntryDto?>
    {
        private readonly IOCREntryQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMiscMasterQueryRepository _misc;
        private readonly IOCREntryFileStorage _fileStorage;

        public GetOCREntryByIdQueryHandler(
            IOCREntryQueryRepository queryRepository,
            IMediator mediator,
            IMiscMasterQueryRepository misc,
            IOCREntryFileStorage fileStorage)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
            _misc = misc;
            _fileStorage = fileStorage;
        }

        public async Task<OCREntryDto?> Handle(GetOCREntryByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            // Build the full document URL: {baseUrl}/Purchase/OCRDocuments/{company}/{unit}/{file}.
            // baseUrl is the Description of the "ImagePath" MiscType (Purchase.MiscTypeMaster).
            if (!string.IsNullOrWhiteSpace(result.DocumentPath))
            {
                try
                {
                    var baseUrl = await _misc.GetMiscTypeDescriptionAsync(MiscEnumEntity.ImagePath) ?? string.Empty;

                    result.DocumentFullPath = await _fileStorage.BuildPublicUrlAsync(
                        baseUrl, result.DocumentPath, cancellationToken);
                }
                catch { /* swallow full-path enrichment failures */ }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetOCREntryByIdQuery",
                actionName: result.Id.ToString(),
                details: $"OCR details {result.Id} was fetched.",
                module: "OCREntry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
