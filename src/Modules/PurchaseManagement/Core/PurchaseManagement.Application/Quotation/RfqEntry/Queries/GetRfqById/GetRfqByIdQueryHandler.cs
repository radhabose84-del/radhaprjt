using AutoMapper;
using Contracts.Dtos.Inventory;
using Contracts.Interfaces.External.IInvetoryManagement;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqById;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;
using System.Linq;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries;

public class GetRfqByIdQueryHandler : IRequestHandler<GetRfqByIdQuery, RfqDto>
{
    private readonly IRfqQueryRepository _repo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IItemGrpcClient _itemGrpcClient;
    private readonly IUOMLookup _uomLookup;
    private readonly IHSNLookup _hsnLookup;

    public GetRfqByIdQueryHandler(
        IRfqQueryRepository repo,
        IMapper mapper,
        IMediator mediator,
        IItemGrpcClient itemGrpcClient,
        IUOMLookup uomLookup,
        IHSNLookup hsnLookup)
    {
        _repo = repo;
        _mapper = mapper;
        _mediator = mediator;
        _itemGrpcClient = itemGrpcClient;
        _uomLookup = uomLookup;
        _hsnLookup = hsnLookup;
    }

    public async Task<RfqDto> Handle(GetRfqByIdQuery request, CancellationToken ct)
    {
        var agg = await _repo.GetAggregateAsync(request.Id, ct)
                  ?? throw new ExceptionRules("RFQ not found.");

        var result = _mapper.Map<RfqDto>(agg);

        if (result.Items == null || result.Items.Length == 0)
        {
            await PublishAudit(result, ct);
            return result;
        }

        // Distinct IDs for lookups
        var itemIds = result.Items.Select(i => i.ItemId).Where(id => id > 0).Distinct().ToList();
        var uomIds  = result.Items.Select(i => i.UomId).Where(id => id > 0).Distinct().ToList();
        var hsnIds  = result.Items.Select(i => i.HsnId).Where(id => id > 0).Distinct().ToList();

        // Parallel lookups
        var itemsTask = _itemGrpcClient.GetItemsByIdsAsync(itemIds, ct);                       // List<ItemMasterDto>
        var uomTask   = _uomLookup.GetByIdsAsync(uomIds, ct);                                 // IReadOnlyList<UOMLookupDto>
        var hsnTask   = _hsnLookup.GetByIdsAsync(hsnIds, ct);                                 // IReadOnlyList<HSNLookupDto>

        await Task.WhenAll(itemsTask, uomTask, hsnTask);

        // Build maps
        var itemsFromGrpc = await itemsTask; // List<ItemMasterDto>
        var itemDetailMap = itemsFromGrpc
            .GroupBy(x => x.Id)
            .ToDictionary(g => g.Key, g => g.First());

        var itemNameMap = itemsFromGrpc
            .GroupBy(x => x.Id)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var it = g.First();
                    return string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemCode : it.ItemName;
                });

        var uomMap = (await uomTask)
            .GroupBy(u => u.Id)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var u = g.First();
                    return string.IsNullOrWhiteSpace(u.UOMName)
                        ? (u.Code ?? u.Id.ToString())
                        : u.UOMName;
                });

        var hsnMap = (await hsnTask)
            .GroupBy(h => h.Id)
            .ToDictionary(g => g.Key, g => g.First()); // HSN Id -> HSN dto (has GSTPercentage)

        // Rebuild items with names, GST & ItemCategoryId from gRPC (when missing in RFQ)
        var enrichedItems = result.Items.Select(i =>
        {
            itemNameMap.TryGetValue(i.ItemId, out var itemName);
            uomMap.TryGetValue(i.UomId, out var uomName);
            itemDetailMap.TryGetValue(i.ItemId, out var grpcItem);

            // GST priority: HSN (if available) -> RFQ -> gRPC item
            decimal gst = i.GstPercentage;
            if (i.HsnId > 0 && hsnMap.TryGetValue(i.HsnId, out var hsnDto))
                gst = hsnDto.GSTPercentage;
            else if (gst <= 0 && grpcItem is not null)
                gst = grpcItem.GSTPercentage;

            // ItemCategoryId: RFQ if present, else gRPC item
            int itemCategoryId = i.ItemCategoryId > 0
                ? i.ItemCategoryId
                : (grpcItem?.ItemCategoryId ?? 0);

            return new RfqItemDto(
                i.ItemId,
                i.Qty,
                i.UomId,
                uomName ?? string.Empty,
                itemName ?? string.Empty,
                gst,
                i.HsnId,
                itemCategoryId
            );
        }).ToArray();

        var enriched = result with { Items = enrichedItems };

        await PublishAudit(enriched, ct);
        return enriched;
    }

    private async Task PublishAudit(RfqDto dto, CancellationToken ct)
    {
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: string.Empty,
            actionName: string.Empty,
            details: $"Rfq details {dto.Id} was fetched.",
            module: "RFQ");
        await _mediator.Publish(domainEvent, ct);
    }
}
