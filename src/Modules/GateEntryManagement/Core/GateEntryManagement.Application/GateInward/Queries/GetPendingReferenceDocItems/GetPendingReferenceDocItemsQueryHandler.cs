using Contracts.Common;
using Contracts.Dtos.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Gate;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Queries.GetPendingReferenceDocItems
{
    public class GetPendingReferenceDocItemsQueryHandler
        : IRequestHandler<GetPendingReferenceDocItemsQuery, ApiResponseDTO<List<PendingReferenceDocLineDto>>>
    {
        private readonly IEnumerable<IPendingReferenceDocResolver> _resolvers;
        private readonly ITransactionTypeLookup _transactionTypeLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IItemPurchaseToleranceLookup _itemPurchaseToleranceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetPendingReferenceDocItemsQueryHandler(
            IEnumerable<IPendingReferenceDocResolver> resolvers,
            ITransactionTypeLookup transactionTypeLookup,
            IItemLookup itemLookup,
            IItemPurchaseToleranceLookup itemPurchaseToleranceLookup,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _resolvers = resolvers;
            _transactionTypeLookup = transactionTypeLookup;
            _itemLookup = itemLookup;
            _itemPurchaseToleranceLookup = itemPurchaseToleranceLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<PendingReferenceDocLineDto>>> Handle(
            GetPendingReferenceDocItemsQuery request, CancellationToken cancellationToken)
        {
            var poIds = request.PoIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (poIds.Count == 0)
            {
                return new ApiResponseDTO<List<PendingReferenceDocLineDto>>
                {
                    IsSuccess = false,
                    Message = "PoIds is required.",
                    Data = new List<PendingReferenceDocLineDto>()
                };
            }

            // 1) FE's TransactionTypeId → ShortName via lookup (cached). Single source of truth.
            var txnTypes = await _transactionTypeLookup.GetByIdsAsync(new[] { request.ReferenceDocumentTypeId });
            var txnType = txnTypes.FirstOrDefault();
            if (txnType == null || string.IsNullOrWhiteSpace(txnType.ShortName))
            {
                return new ApiResponseDTO<List<PendingReferenceDocLineDto>>
                {
                    IsSuccess = false,
                    Message = $"Reference document type Id={request.ReferenceDocumentTypeId} not found in Finance.TransactionTypeMaster.",
                    Data = new List<PendingReferenceDocLineDto>()
                };
            }

            // 2) Dispatch to the resolver whose DocumentTypeCode matches.
            var resolver = _resolvers.FirstOrDefault(r =>
                string.Equals(r.DocumentTypeCode, txnType.ShortName, StringComparison.OrdinalIgnoreCase));

            if (resolver == null)
            {
                return new ApiResponseDTO<List<PendingReferenceDocLineDto>>
                {
                    IsSuccess = false,
                    Message = $"No pending-document resolver is registered for DocumentTypeCode = '{txnType.ShortName}'.",
                    Data = new List<PendingReferenceDocLineDto>()
                };
            }

            // 3) Fetch line items grouped by PO.
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var groups = (await resolver.GetPendingItemsAsync(poIds, request.PartyId, unitId, cancellationToken)).ToList();

            // 4) Stamp doc-type metadata from the lookup row.
            foreach (var g in groups)
            {
                g.TransactionTypeId = txnType.Id;
                g.DocumentTypeCode = txnType.ShortName;
            }

            // 5) Enrich item rows from TWO lookups in parallel:
            //    - IItemLookup (canonical Inventory.ItemMaster with IsDeleted=0 filter) → ItemCode, ItemName
            //    - IItemPurchaseToleranceLookup (ItemInventory + ItemPurchase + UOM) → UOMName, Upper/LowerTolerance
            //    Both are cached globally by AddLookupCaching, so repeat calls within 30 min are dictionary reads.
            var itemIds = groups.SelectMany(g => g.Items)
                                .Select(i => i.ItemId)
                                .Where(id => id > 0)
                                .Distinct()
                                .ToList();
            if (itemIds.Count > 0)
            {
                var itemTask = _itemLookup.GetByIdsAsync(itemIds, cancellationToken);
                var toleranceTask = _itemPurchaseToleranceLookup.GetByIdsAsync(itemIds, cancellationToken);
                await Task.WhenAll(itemTask, toleranceTask);

                var itemMap = (await itemTask).ToDictionary(i => i.Id, i => i);
                var tolMap  = (await toleranceTask).ToDictionary(t => t.ItemId, t => t);

                foreach (var g in groups)
                {
                    foreach (var item in g.Items)
                    {
                        // ItemCode / ItemName — prefer IItemLookup; fallback to tolerance lookup.
                        if (itemMap.TryGetValue(item.ItemId, out var itemDto) && itemDto is not null)
                        {
                            if (!string.IsNullOrWhiteSpace(itemDto.ItemCode)) item.ItemCode = itemDto.ItemCode;
                            if (!string.IsNullOrWhiteSpace(itemDto.ItemName)) item.ItemName = itemDto.ItemName;
                        }

                        // Tolerances + UOMName from tolerance lookup; also fills code/name if IItemLookup missed.
                        if (tolMap.TryGetValue(item.ItemId, out var tol) && tol is not null)
                        {
                            item.UpperTolerance = tol.UpperTolerance;
                            item.LowerTolerance = tol.LowerTolerance;
                            if (!string.IsNullOrWhiteSpace(tol.UOMName))  item.UOMName  = tol.UOMName;
                            if (string.IsNullOrWhiteSpace(item.ItemCode) && !string.IsNullOrWhiteSpace(tol.ItemCode))
                                item.ItemCode = tol.ItemCode;
                            if (string.IsNullOrWhiteSpace(item.ItemName) && !string.IsNullOrWhiteSpace(tol.ItemName))
                                item.ItemName = tol.ItemName;
                        }
                    }
                }
            }

            // 6) Preserve FE multi-select order: order POs by their position in request.PoIds.
            var orderMap = poIds
                .Select((id, idx) => new { id, idx })
                .ToDictionary(x => x.id, x => x.idx);
            var ordered = groups
                .OrderBy(g => orderMap.TryGetValue(g.DocId, out var idx) ? idx : int.MaxValue)
                .ToList();

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetPendingReferenceDocItemsQuery",
                actionName: ordered.Count.ToString(),
                details: $"Pending ref-doc line items fetched (TxnType={txnType.Id}/{txnType.ShortName}, Party={request.PartyId}, Pos={string.Join(',', poIds)}).",
                module: "GateInward");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<List<PendingReferenceDocLineDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = ordered
            };
        }
    }
}
