using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOById
{
    public class GetRawMaterialPOByIdQueryHandler : IRequestHandler<GetRawMaterialPOByIdQuery, RawMaterialPODto?>
    {
        private readonly IRawMaterialPOQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMiscMasterQueryRepository _misc;
        private readonly IRawMaterialPOFileStorage _fileStorage;

        public GetRawMaterialPOByIdQueryHandler(
            IRawMaterialPOQueryRepository queryRepository,
            IMediator mediator,
            IMiscMasterQueryRepository misc,
            IRawMaterialPOFileStorage fileStorage)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
            _misc = misc;
            _fileStorage = fileStorage;
        }

        public async Task<RawMaterialPODto?> Handle(GetRawMaterialPOByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            // Build the full document URL: {baseUrl}/Purchase/RawMaterialPODocuments/{company}/{unit}/{file}.
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
                actionCode: "GetRawMaterialPOByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Raw Material PO details {result.Id} was fetched.",
                module: "RawMaterialPO");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
