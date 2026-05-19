using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using PurchaseManagement.Domain.Entities.ValueObjects;
using MediatR;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Commands.UpsertDraft;

public class UpsertRfqDraftCommandHandler : IRequestHandler<UpsertRfqDraftCommand, UpsertRfqDraftResult>
{
    private readonly IRfqCommandRepository _repo;
    private readonly IIPAddressService _ip;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;

    public UpsertRfqDraftCommandHandler(IRfqCommandRepository repo, IIPAddressService ip,
        IDocumentSequenceLookup documentSequenceLookup)
    {
        _repo = repo;
        _ip = ip;
        _documentSequenceLookup = documentSequenceLookup;
    }

    public async Task<UpsertRfqDraftResult> Handle(UpsertRfqDraftCommand request, CancellationToken ct)
    {
        static bool IsMeaningfulItem(DraftItemDto i) =>
            i is not null && i.ItemId > 0 && i.UomId > 0; // qty can be 0 in draft

        static bool IsMeaningfulSupplier(DraftSupplierDto s) =>
            s is not null && ((s.SupplierId ?? 0) > 0 ||
                              !string.IsNullOrWhiteSpace(s.Name) ||
                              !string.IsNullOrWhiteSpace(s.Email));

        static EmailAddress? SafeEmailOrNull(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            try { return new EmailAddress(s.Trim()); }
            catch { return null; } // do NOT throw on draft
        }

        // -------------------------------------------------
        // CREATE DRAFT
        // -------------------------------------------------
        if (request.Id is null)
        {
            var unitId = _ip.GetUnitId() ?? 0;

            // Generate RfqCode from DocumentSequence
            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeRFQ, MiscEnumEntity.ModulePurchase, unitId)
                ?? throw new InvalidOperationException("No transaction type configured for RFQ.");
            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
            var code = sequences.Count > 0
                ? sequences[^1]
                : throw new InvalidOperationException("No document sequence configured for RFQ.");

            var draftStatusId = await _repo.GetStatusIdByCodeAsync("DRAFT", ct);

            var rfq = new RfqMaster
            {
                RfqCode          = code,
                UnitId           = unitId,
                InitiationTypeId = request.InitiationTypeId ?? 0,
                IndentId         = request.IndentId,
                RfqStatusId      = draftStatusId,
                CreatedDate      = DateTimeOffset.UtcNow
            };

            if (request.Items is not null)
                rfq.Items = request.Items
                    .Where(IsMeaningfulItem)
                    .Select(i => new RfqItem
                    {
                        ItemId   = i.ItemId,
                        HsnId    = i.HsnId,
                        Quantity = i.Qty,
                        UomId    = i.UomId
                    }).ToList();

            if (request.Suppliers is not null)
                rfq.Suppliers = request.Suppliers
                    .Where(IsMeaningfulSupplier)
                    .Select(s => new RfqSupplier
                    {
                        SupplierId = s.SupplierId,
                        Name       = s.Name ?? string.Empty,
                        Email      = SafeEmailOrNull(s.Email),
                        Mobile     = s.Mobile,
                        GSTNumber  = s.Gst
                    }).ToList();

            var id = await _repo.CreateAsync(rfq, transactionTypeId, ct);
            return new UpsertRfqDraftResult(id, rfq.RfqCode);
        }

        // -------------------------------------------------
        // UPDATE EXISTING DRAFT (PATCH: update + add, no delete)
        // -------------------------------------------------
        var header = new RfqMaster
        {
            Id               = request.Id.Value,
            UnitId           = _ip.GetUnitId() ?? 0,
            InitiationTypeId = request.InitiationTypeId ?? 0,
            IndentId         = request.IndentId
        };

        // Here we PRESERVE incoming Id to allow patch
        List<RfqItem>? items = request.Items?
            .Where(IsMeaningfulItem)
            .Select(i => new RfqItem
            {
                Id       = i.Id ?? 0,              // 👈 keep Id (0 = new)
                RfqId    = request.Id.Value,
                ItemId   = i.ItemId,
                HsnId    = i.HsnId,
                Quantity = i.Qty,
                UomId    = i.UomId
            }).ToList();

        List<RfqSupplier>? suppliers = request.Suppliers?
            .Where(IsMeaningfulSupplier)
            .Select(s => new RfqSupplier
            {
                Id         = s.Id ?? 0,           // 👈 keep Id (0 = new)
                RfqId      = request.Id.Value,
                SupplierId = s.SupplierId,
                Name       = s.Name ?? string.Empty,
                Email      = SafeEmailOrNull(s.Email),
                Mobile     = s.Mobile,
                GSTNumber  = s.Gst
            }).ToList();

        await _repo.UpdateDraftPartialAsync(
            id: request.Id.Value,
            headerAfter: header,
            desiredItems: items,          // null => keep existing
            desiredSuppliers: suppliers,  // null => keep existing
            ct: ct);

        var updated = await _repo.GetAggregateTrackingAsync(request.Id.Value, ct);
        return new UpsertRfqDraftResult(updated!.Id, updated.RfqCode);
    }
}
