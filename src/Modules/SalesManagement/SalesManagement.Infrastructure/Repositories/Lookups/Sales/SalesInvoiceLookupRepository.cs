using System.Data;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class SalesInvoiceLookupRepository : ISalesInvoiceLookup
{
    private readonly IDbConnection _dbConnection;
    private readonly IPartyDetailLookup _partyDetailLookup;
    private readonly IItemLookup _itemLookup;
    private readonly IUOMLookup _uomLookup;

    public SalesInvoiceLookupRepository(
        IDbConnection dbConnection,
        IPartyDetailLookup partyDetailLookup,
        IItemLookup itemLookup,
        IUOMLookup uomLookup)
    {
        _dbConnection = dbConnection;
        _partyDetailLookup = partyDetailLookup;
        _itemLookup = itemLookup;
        _uomLookup = uomLookup;
    }

    public async Task<SalesInvoiceForEInvoiceDto?> GetInvoiceForEInvoiceAsync(string invoiceNumber, int unitId)
    {
        const string sql = @"
            SELECT h.Id, h.InvoiceNo, h.InvoiceDate, h.UnitId, h.PartyId,
                h.TaxableValue, h.TotalDiscount, h.TotalFreight, h.Insurance,
                h.HandlingCharge, h.OtherCharges,
                h.CGST, h.SGST, h.IGST, h.TCS, h.RoundOff,
                h.InvoiceAmount, h.Remarks,
                da.TransporterId,
                COALESCE(NULLIF(TRIM(da.VehicleNo), ''), NULLIF(TRIM(h.VehicleNumber), '')) AS VehicleNo,
                h.TransportMode,
                mm.Code AS TransportModeCode,
                h.TransporterName,
                so.SalesOrderTypeId
            FROM Sales.InvoiceHeader h
            LEFT JOIN Sales.DispatchAdviceHeader da
                ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
            LEFT JOIN Sales.SalesOrderHeader so
                ON da.SalesOrderId = so.Id AND so.IsDeleted = 0
            LEFT JOIN Sales.MiscMaster mm
                ON h.TransportMode = mm.Id AND mm.IsDeleted = 0
            WHERE h.InvoiceNo = @InvoiceNo AND h.UnitId = @UnitId AND h.IsDeleted = 0";

        var header = await _dbConnection.QueryFirstOrDefaultAsync<SalesInvoiceForEInvoiceDto>(
            sql, new { InvoiceNo = invoiceNumber, UnitId = unitId });

        if (header == null)
            return null;

        return await PopulateDetailsAndLookupsAsync(header);
    }

    public async Task<SalesInvoiceForEInvoiceDto?> GetInvoiceForEInvoiceByIdAsync(int invoiceId)
    {
        const string sql = @"
            SELECT h.Id, h.InvoiceNo, h.InvoiceDate, h.UnitId, h.PartyId,
                h.TaxableValue, h.TotalDiscount, h.TotalFreight, h.Insurance,
                h.HandlingCharge, h.OtherCharges,
                h.CGST, h.SGST, h.IGST, h.TCS, h.RoundOff,
                h.InvoiceAmount, h.Remarks,
                da.TransporterId,
                COALESCE(NULLIF(TRIM(da.VehicleNo), ''), NULLIF(TRIM(h.VehicleNumber), '')) AS VehicleNo,
                h.TransportMode,
                mm.Code AS TransportModeCode,
                h.TransporterName,
                so.SalesOrderTypeId
            FROM Sales.InvoiceHeader h
            LEFT JOIN Sales.DispatchAdviceHeader da
                ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
            LEFT JOIN Sales.SalesOrderHeader so
                ON da.SalesOrderId = so.Id AND so.IsDeleted = 0
            LEFT JOIN Sales.MiscMaster mm
                ON h.TransportMode = mm.Id AND mm.IsDeleted = 0
            WHERE h.Id = @Id AND h.IsDeleted = 0";

        var header = await _dbConnection.QueryFirstOrDefaultAsync<SalesInvoiceForEInvoiceDto>(
            sql, new { Id = invoiceId });

        if (header == null)
            return null;

        return await PopulateDetailsAndLookupsAsync(header);
    }

    private async Task<SalesInvoiceForEInvoiceDto> PopulateDetailsAndLookupsAsync(SalesInvoiceForEInvoiceDto header)
    {
        // Fetch detail lines
        const string detailSql = @"
            SELECT d.ItemSno, d.ItemId, d.HsnCode, d.NoOfBags, d.NetWeight,
                d.RatePerKg, d.DiscountValue, d.TaxableAmount, d.GstPercentage,
                d.CGST, d.SGST, d.IGST, d.TotalAmount,
                d.PackTypeId, d.UOMId
            FROM Sales.InvoiceDetail d
            WHERE d.InvoiceHeaderId = @HeaderId
            ORDER BY d.ItemSno";

        var details = (await _dbConnection.QueryAsync<SalesInvoiceDetailForEInvoiceDto>(
            detailSql, new { HeaderId = header.Id })).ToList();

        // Populate party GST details via cross-module lookup
        var party = await _partyDetailLookup.GetByIdAsync(header.PartyId);
        if (party != null)
        {
            header.GstNo = party.GSTNumber;
            header.ReverseCharge = party.IsGstReverseCharge;
            header.PlaceOfSupply = party.GSTStateCode?.ToString("D2");
        }

        // Populate transporter GSTIN via cross-module lookup; keep InvoiceHeader TransporterName if set
        if (header.TransporterId.HasValue && header.TransporterId.Value > 0)
        {
            var transporter = await _partyDetailLookup.GetByIdAsync(header.TransporterId.Value);
            header.TransporterGstin = transporter?.GSTNumber;
            if (string.IsNullOrWhiteSpace(header.TransporterName))
                header.TransporterName = transporter?.PartyName;
        }

        // Populate item names and UOM via cross-module lookups
        if (details.Count > 0)
        {
            var itemIds = details.Select(d => d.ItemId).Distinct();
            var items = await _itemLookup.GetByIdsAsync(itemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

            var uomIds = details.Where(d => d.UOMId.HasValue).Select(d => d.UOMId!.Value).Distinct();
            var uoms = await _uomLookup.GetByIdsAsync(uomIds);
            var uomDict = uoms.ToDictionary(u => u.Id, u => u.UOMName);

            foreach (var detail in details)
            {
                detail.ItemName = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;
                detail.UOMName = detail.UOMId.HasValue && uomDict.TryGetValue(detail.UOMId.Value, out var uName) ? uName : null;
            }
        }

        header.Details = details;
        return header;
    }

    public async Task RevertInvoiceStatusToPendingAsync(int invoiceId, CancellationToken ct)
    {
        const string sql = @"
            DECLARE @PendingStatusId INT;
            SELECT TOP 1 @PendingStatusId = mm.Id
            FROM Sales.MiscMaster mm
            INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
            WHERE mt.MiscTypeCode = 'ApprovalStatus' AND mm.Code = 'Pending' AND mm.IsDeleted = 0;

            IF @PendingStatusId IS NOT NULL
                UPDATE Sales.InvoiceHeader
                SET StatusId = @PendingStatusId
                WHERE Id = @InvoiceId AND IsDeleted = 0;";

        await _dbConnection.ExecuteAsync(
            new CommandDefinition(sql, new { InvoiceId = invoiceId }, cancellationToken: ct));
    }
}
