using System.Data;
using Contracts.Dtos.Purchase;
using Contracts.Interfaces.Purchase;
using Dapper;
using FluentValidation;
using MediatR;
using PurchaseManagement.Application.GRN.GRNEntry.Commands;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;

namespace PurchaseManagement.Infrastructure.Services
{
    /// <summary>
    /// Cross-module adapter that lets the centralized Gate Inward flow drive the existing GRN
    /// command pipeline. The Gate handler passes the leanest possible context (PoId + PoSlNoLocal
    /// + DcQuantity per line); this bridge enriches the lines from Purchase + Inventory tables
    /// (single source of truth — no snapshot duplication on the Gate side) before building
    /// the internal <see cref="CreateGRNEntryCommand"/>.
    /// </summary>
    internal sealed class GateInwardGrnBridge : IGateInwardGrnBridge
    {
        private readonly IDbConnection _dbConnection;
        private readonly IMediator _mediator;
        private readonly IValidator<CreateGRNEntryCommand> _grnValidator;

        public GateInwardGrnBridge(
            IDbConnection dbConnection,
            IMediator mediator,
            IValidator<CreateGRNEntryCommand> grnValidator)
        {
            _dbConnection = dbConnection;
            _mediator = mediator;
            _grnValidator = grnValidator;
        }

        public async Task<IReadOnlyList<string>> ValidateAsync(
            GateInwardGrnContextDto input, CancellationToken ct = default)
        {
            var command = await BuildCommandAsync(input, ct);
            var result = await _grnValidator.ValidateAsync(command, ct);
            return result.IsValid
                ? Array.Empty<string>()
                : result.Errors.Select(e => e.ErrorMessage).ToList();
        }

        public async Task<int> CreateAsync(
            GateInwardGrnContextDto input, CancellationToken ct = default)
        {
            var command = await BuildCommandAsync(input, ct);
            return await _mediator.Send(command, ct);
        }

        // ─── helpers ────────────────────────────────────────────────────────────────

        private async Task<CreateGRNEntryCommand> BuildCommandAsync(
            GateInwardGrnContextDto input, CancellationToken ct)
        {
            if (input.Lines.Count == 0)
            {
                return ToCommand(input, new List<CreateGRNEntryDto.CreateGRNDetailsDto>());
            }

            // Enrich the lean (PoId, PoSlNoLocal, DcQuantity) rows with PO + Item context.
            var enrichments = await EnrichAsync(input.DocumentTypeCode, input.Lines, ct);
            var enrichmentMap = enrichments.ToDictionary(e => (e.PoId, e.PoSlNoLocal ?? 0), e => e);

            var details = new List<CreateGRNEntryDto.CreateGRNDetailsDto>(input.Lines.Count);
            foreach (var line in input.Lines)
            {
                if (!enrichmentMap.TryGetValue((line.PoId, line.PoSlNoLocal ?? 0), out var e))
                {
                    // Skip lines we couldn't enrich (caller already pre-validated, so this should be rare).
                    continue;
                }

                details.Add(new CreateGRNEntryDto.CreateGRNDetailsDto
                {
                    PoId = line.PoId,
                    PoSlNoLocal = line.PoSlNoLocal,
                    PoCategoryId = e.PoCategoryId,
                    PoMethodId = e.PoMethodId,
                    ItemId = e.ItemId,
                    OrderQuantity = e.OrderQuantity,
                    DcQuantity = line.DcQuantity,
                    UpperTolerance = e.UpperTolerance,
                    LowerTolerance = e.LowerTolerance,
                    ReceivedQuantity = line.DcQuantity,   // default per design
                    ExpiryDate = line.ExpiryDate,         // user-supplied; null = unknown
                    BatchNumber = null,
                    GrnDetailImage = null
                });
            }

            return ToCommand(input, details);
        }

        private static CreateGRNEntryCommand ToCommand(
            GateInwardGrnContextDto input, List<CreateGRNEntryDto.CreateGRNDetailsDto> details) =>
            new()
            {
                GrnEntryCreate = new CreateGRNEntryDto
                {
                    UnitId = input.UnitId,
                    GateEntryId = input.GateEntryId,
                    PartyId = input.PartyId,
                    InvoiceNo = input.InvoiceNo,
                    InvoiceDate = input.InvoiceDate,
                    DcNo = input.DcNo,
                    DcDate = input.DcDate,
                    ReceivingWarehouseId = input.ReceivingWarehouseId,
                    Remarks = input.Remarks,
                    IsGrnGenerated = 1,
                    GrnReceivedImage = null,
                    GRNDetailsDtos = details
                }
            };

        // Re-fetches everything we removed from Gate.GateInwardDtl. One Dapper query per doc-type,
        // ranged by (PoId, PoSlNoLocal). Single source of truth: PO sub-detail + Inventory.ItemInventory.
        private async Task<IReadOnlyList<EnrichmentRow>> EnrichAsync(
            string? documentTypeCode, IList<GateInwardGrnLineDto> lines, CancellationToken ct)
        {
            // Build the (PoId, PoSlNoLocal) key list once.
            var keys = lines
                .Select(l => new { l.PoId, PoSlNoLocal = l.PoSlNoLocal ?? 0 })
                .Distinct()
                .ToList();
            if (keys.Count == 0)
                return Array.Empty<EnrichmentRow>();

            var poIds = keys.Select(k => k.PoId).Distinct().ToList();
            var slNos = keys.Select(k => k.PoSlNoLocal).Distinct().ToList();

            // Doc-type chooses the sub-detail table — same split as the resolvers.
            // LPO → PurchaseLocalHeader + PurchaseLocalDetail
            // IPO → PurchaseOrderImportHeader + PurchaseOrderImportDetail
            string sql;
            switch ((documentTypeCode ?? string.Empty).ToUpperInvariant())
            {
                case "LPO":
                    sql = @"
                        SELECT
                            poh.Id          AS PoId,
                            pld.ItemSno     AS PoSlNoLocal,
                            poh.POCategoryId AS PoCategoryId,
                            poh.POMethodId  AS PoMethodId,
                            pld.ItemId      AS ItemId,
                            pld.Quantity    AS OrderQuantity,
                            ii.UpperTolerance AS UpperTolerance,
                            ii.LowerTolerance AS LowerTolerance
                        FROM Purchase.PurchaseOrderHeader poh
                        INNER JOIN Purchase.PurchaseLocalHeader plh ON poh.Id = plh.PurchaseOrderId
                        INNER JOIN Purchase.PurchaseLocalDetail  pld ON plh.Id = pld.PurchaseLocalId
                        LEFT  JOIN Inventory.ItemInventory ii        ON ii.ItemId = pld.ItemId
                        WHERE poh.Id IN @PoIds
                          AND pld.ItemSno IN @SlNos
                          AND poh.IsDeleted = 0;";
                    break;

                case "IPO":
                    sql = @"
                        SELECT
                            poh.Id           AS PoId,
                            poid.ItemSno     AS PoSlNoLocal,
                            poh.POCategoryId AS PoCategoryId,
                            poh.POMethodId   AS PoMethodId,
                            poid.ItemId      AS ItemId,
                            poid.Quantity    AS OrderQuantity,
                            ii.UpperTolerance AS UpperTolerance,
                            ii.LowerTolerance AS LowerTolerance
                        FROM Purchase.PurchaseOrderHeader poh
                        INNER JOIN Purchase.PurchaseOrderImportHeader poih ON poh.Id  = poih.PurchaseOrderId
                        INNER JOIN Purchase.PurchaseOrderImportDetail  poid ON poih.Id = poid.PurchaseHeaderId
                        LEFT  JOIN Inventory.ItemInventory ii               ON ii.ItemId = poid.ItemId
                        WHERE poh.Id IN @PoIds
                          AND poid.ItemSno IN @SlNos
                          AND poh.IsDeleted = 0;";
                    break;

                default:
                    // Unknown doc-type — caller will have already failed validation upstream.
                    return Array.Empty<EnrichmentRow>();
            }

            var rows = await _dbConnection.QueryAsync<EnrichmentRow>(
                new CommandDefinition(sql, new { PoIds = poIds, SlNos = slNos }, cancellationToken: ct));
            return rows.ToList();
        }

        private sealed class EnrichmentRow
        {
            public int PoId { get; set; }
            public int? PoSlNoLocal { get; set; }
            public int PoCategoryId { get; set; }
            public int PoMethodId { get; set; }
            public int ItemId { get; set; }
            public decimal OrderQuantity { get; set; }
            public decimal? UpperTolerance { get; set; }
            public decimal? LowerTolerance { get; set; }
        }
    }
}
