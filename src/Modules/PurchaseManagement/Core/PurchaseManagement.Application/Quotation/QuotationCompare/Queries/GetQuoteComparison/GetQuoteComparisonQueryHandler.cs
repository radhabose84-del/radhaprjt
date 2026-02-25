using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;

namespace PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparison
{
    public class GetQuoteComparisonQueryHandler : IRequestHandler<GetQuoteComparisonQuery, QuoteComparisonDto?>
    {
        private readonly IQuotationCompareQueryRepository _quotationCompareQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;


        public GetQuoteComparisonQueryHandler(
            IQuotationCompareQueryRepository quotationCompareQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IItemLookup itemLookup,
            IUOMLookup uomLookup)
        {
            _quotationCompareQueryRepository = quotationCompareQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
        }
        public async Task<QuoteComparisonDto?> Handle(GetQuoteComparisonQuery request, CancellationToken cancellationToken)
        {
            // Step 1: Get base data from repository (without ItemName, UOM)
            var result = await _quotationCompareQueryRepository
                .GetQuoteComparisonAsync(request.RfqId, cancellationToken);

            if (result == null)
                return null;

            var quoteComparison = _mapper.Map<QuoteComparisonDto>(result);

            // Step 2: Collect ItemIds and UomIds for gRPC lookup
            var itemIds = quoteComparison.Items.Select(i => i.ItemId).Distinct().ToList();
            var uomIds = quoteComparison.Items.Select(i => i.UomId).Distinct().ToList();

            // Step 3: Kick off gRPC calls in parallel
            var itemsTask = _itemLookup.GetByIdsAsync(itemIds, cancellationToken);
            var uomsTask = _uomLookup.GetAllAsync();

            await Task.WhenAll(itemsTask, uomsTask);

            // Step 4: Build dictionaries
            var itemsDict = (await itemsTask).ToDictionary(i => i.Id, i => i.ItemName);
            var uomsDict = (await uomsTask).ToDictionary(u => u.Id, u => u.UOMName);

            // Step 5: Map names into DTO
            foreach (var item in quoteComparison.Items)
            {
                if (itemsDict.TryGetValue(item.ItemId, out var itemName))
                    item.ItemName = itemName;

                if (uomsDict.TryGetValue(item.UomId, out var uomName))
                    item.Uom = uomName;
            }

            // Step 6: Apply Supplier Flags
            foreach (var item in quoteComparison.Items)
            {
                if (item.Suppliers.Any())
                {
                    var minTotal = item.Suppliers.Min(s => s.Total);
                    var minDelivery = item.Suppliers.Min(s => s.DeliveryDays);

                    // Select suggested supplier: lowest cost, then tie-breaker by delivery
                    var suggestedSupplier = item.Suppliers
                        .OrderBy(s => s.Total)
                        .ThenBy(s => s.DeliveryDays)
                        .FirstOrDefault();

                    foreach (var supplier in item.Suppliers)
                    {
                        // Suggested = the chosen one
                        supplier.IsSuggested = (supplier == suggestedSupplier);

                        // Discount flag
                        supplier.IsDiscount = supplier.DiscountValue > 0;

                        // Expired flag
                        supplier.IsExpired = supplier.ValidTill < DateTime.Now;

                        // Fastest delivery
                        supplier.IsFastest = supplier.DeliveryDays == minDelivery;
                    }
                }
            }

            // Step 5: Publish audit log
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetQuoteComparisonQuery",
                actionName: quoteComparison.RfqId.ToString(),
                details: $"Quote comparison for RFQ {quoteComparison.RfqId} was fetched.",
                module: "QuotationCompare"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return quoteComparison;
        }

    }
}