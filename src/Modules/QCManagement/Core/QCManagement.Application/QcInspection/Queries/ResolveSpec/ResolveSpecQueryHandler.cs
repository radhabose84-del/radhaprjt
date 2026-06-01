using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Purchase;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.QcInspection.Queries.ResolveSpec
{
    public class ResolveSpecQueryHandler : IRequestHandler<ResolveSpecQuery, ResolveSpecPreviewDto>
    {
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IGrnLookup _grnLookup;
        private readonly IItemLookup _itemLookup;

        public ResolveSpecQueryHandler(
            IQcInspectionQueryRepository queryRepository,
            IGrnLookup grnLookup,
            IItemLookup itemLookup)
        {
            _queryRepository = queryRepository;
            _grnLookup = grnLookup;
            _itemLookup = itemLookup;
        }

        public async Task<ResolveSpecPreviewDto> Handle(ResolveSpecQuery request, CancellationToken cancellationToken)
        {
            var grn = await _grnLookup.GetByGrnDetailIdAsync(request.GrnDetailId, cancellationToken);
            if (grn == null)
                return new ResolveSpecPreviewDto { Found = false, Message = "GRN line item not found." };

            var items = await _itemLookup.GetByIdsAsync(new[] { grn.ItemId }, cancellationToken);
            var item = items.FirstOrDefault();

            var qcTypeId = await _queryRepository.GetPurchasedGoodsQcTypeIdAsync();
            if (qcTypeId == null)
                return new ResolveSpecPreviewDto { Found = false, Message = "Purchased-goods QC Type is not configured." };

            var specId = await _queryRepository.ResolveActiveSpecIdAsync(grn.ItemId, item?.ItemCategoryId, qcTypeId.Value, DateTimeOffset.UtcNow);
            if (specId == null)
                return new ResolveSpecPreviewDto { Found = false, Message = "No active Quality Specification found for the item." };

            var snap = await _queryRepository.GetSpecSnapshotAsync(specId.Value);
            if (snap == null)
                return new ResolveSpecPreviewDto { Found = false, Message = "Specification snapshot could not be loaded." };

            return new ResolveSpecPreviewDto
            {
                Found = true,
                QualitySpecificationId = snap.QualitySpecificationId,
                QualitySpecificationCode = snap.QualitySpecificationCode,
                QualityTemplateId = snap.QualityTemplateId,
                QualityTemplateCode = snap.QualityTemplateCode,
                ParameterCount = snap.Parameters.Count,
                Message = "Resolved."
            };
        }
    }
}
