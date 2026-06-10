using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetPendingPoReferences
{
    public class GetPendingPoReferencesQueryHandler
        : IRequestHandler<GetPendingPoReferencesQuery, IReadOnlyList<PoReferenceLookupDto>>
    {
        private readonly IPurchaseOrderQueryRepository _purchaseOrderQueryRepository;

        public GetPendingPoReferencesQueryHandler(IPurchaseOrderQueryRepository purchaseOrderQueryRepository)
        {
            _purchaseOrderQueryRepository = purchaseOrderQueryRepository;
        }

        public async Task<IReadOnlyList<PoReferenceLookupDto>> Handle(GetPendingPoReferencesQuery request, CancellationToken cancellationToken)
        {
            var (rows, _) = await _purchaseOrderQueryRepository.GetPOPendingAsync(
                page: null, size: null, search: request.Term, poId: null, poMethodId: null, ct: cancellationToken);

            return rows.Select(r => new PoReferenceLookupDto
            {
                Id = r.Id,
                PoNumber = r.PONumber,
                VendorId = r.VendorId,
                VendorName = r.VendorName
            }).ToList();
        }
    }
}
