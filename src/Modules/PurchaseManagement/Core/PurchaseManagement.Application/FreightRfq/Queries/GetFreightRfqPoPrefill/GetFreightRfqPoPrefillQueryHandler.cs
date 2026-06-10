using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqPoPrefill
{
    public class GetFreightRfqPoPrefillQueryHandler : IRequestHandler<GetFreightRfqPoPrefillQuery, FreightRfqPoPrefillDto?>
    {
        private readonly IPurchaseOrderQueryRepository _purchaseOrderQueryRepository;

        public GetFreightRfqPoPrefillQueryHandler(IPurchaseOrderQueryRepository purchaseOrderQueryRepository)
        {
            _purchaseOrderQueryRepository = purchaseOrderQueryRepository;
        }

        public async Task<FreightRfqPoPrefillDto?> Handle(GetFreightRfqPoPrefillQuery request, CancellationToken cancellationToken)
        {
            var (rows, _) = await _purchaseOrderQueryRepository.GetPOPendingAsync(
                page: null, size: null, search: null, poId: request.PoId, poMethodId: null, ct: cancellationToken);

            var po = rows.FirstOrDefault();
            if (po == null)
                return null;

            return new FreightRfqPoPrefillDto
            {
                PoReferenceId = po.Id,
                PoNumber = po.PONumber,
                SupplierId = po.VendorId,
                SupplierName = po.VendorName,
                TotalQuantity = po.Lines?.Sum(l => l.Quantity) ?? 0m
            };
        }
    }
}
