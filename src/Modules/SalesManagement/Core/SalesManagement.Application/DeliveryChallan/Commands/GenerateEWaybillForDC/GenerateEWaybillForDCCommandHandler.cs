using System.Globalization;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.GenerateStandaloneEwb;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Commands.GenerateEWaybillForDC
{
    /// <summary>
    /// Reads a Delivery Challan, derives e-waybill payload, and dispatches
    /// FinanceManagement's CreateEWaybillHeaderCommand via MediatR.
    ///
    /// Idempotency: the IEWaybillLookup cache is global (30 min sliding) so a rapid
    /// re-click could theoretically slip past the "already exists" guard. Acceptable
    /// for v1 — a follow-up uniqueness constraint on Finance.EWaybillHeader(InvoiceNo,
    /// UnitId) would close that window at the DB layer.
    /// </summary>
    public sealed class GenerateEWaybillForDCCommandHandler
        : IRequestHandler<GenerateEWaybillForDCCommand, ApiResponseDTO<GenerateEWaybillResponseDto>>
    {
        private const string SupplyTypeOutward = "Outward";
        private const string SubSupplyTypeForOwnUse = "For Own Use";
        private const string DocumentTypeDeliveryChallan = "Delivery Challan";
        private const int TransactionTypeRegular = 1;
        private const string EwbStatusPending = "Pending";

        private readonly IDeliveryChallanQueryRepository _dcQueryRepo;
        private readonly IEWaybillLookup _eWaybillLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IMediator _mediator;

        public GenerateEWaybillForDCCommandHandler(
            IDeliveryChallanQueryRepository dcQueryRepo,
            IEWaybillLookup eWaybillLookup,
            IUnitLookup unitLookup,
            ICompanyLookup companyLookup,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            IMediator mediator)
        {
            _dcQueryRepo = dcQueryRepo;
            _eWaybillLookup = eWaybillLookup;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<GenerateEWaybillResponseDto>> Handle(
            GenerateEWaybillForDCCommand request, CancellationToken cancellationToken)
        {
            // 1. Load DC
            var dc = await _dcQueryRepo.GetByIdAsync(request.DeliveryChallanId);
            if (dc == null)
                throw new ExceptionRules($"Delivery Challan with Id {request.DeliveryChallanId} not found.");

            if (string.IsNullOrWhiteSpace(dc.DeliveryNumber))
                throw new ExceptionRules("Delivery Challan has no DeliveryNumber — cannot generate e-waybill.");

            // 2. Idempotency: return existing e-waybill if any
            var existing = await _eWaybillLookup.GetByDCAsync(dc.DeliveryNumber, dc.FromPlantId, cancellationToken);
            if (existing != null && existing.Id > 0)
            {
                return new ApiResponseDTO<GenerateEWaybillResponseDto>
                {
                    IsSuccess = true,
                    Message = $"E-waybill already exists for DC {dc.DeliveryNumber}.",
                    Data = new GenerateEWaybillResponseDto
                    {
                        EWaybillHeaderId = existing.Id,
                        DeliveryNumber = dc.DeliveryNumber,
                        EwbStatus = existing.EwbStatus,
                        EwbNumber = existing.EWBNumber,
                        AlreadyExisted = true
                    }
                };
            }

            // 3. Resolve consignor (FromPlant → Company) and consignee (ToPlant → Company)
            var (fromGstin, fromTradeName) = await ResolvePlantGstinAsync(dc.FromPlantId);
            var (toGstin, toTradeName) = await ResolvePlantGstinAsync(dc.ToPlantId);

            // 4. Resolve transporter
            string? transporterGstin = null;
            string? transporterName = dc.TransporterName;
            if (dc.TransporterId > 0)
            {
                var transporter = await _partyLookup.GetByIdAsync(dc.TransporterId, cancellationToken);
                transporterGstin = transporter?.GstNumber;
                transporterName ??= transporter?.PartyName;
            }

            // 5. Build detail rows from DC line items
            var details = await BuildEWaybillDetailsAsync(dc, cancellationToken);

            // 5b. Validate master data BEFORE inserting. NIC rejects EWBs with missing
            // GSTIN / HSN / vehicle / distance, so we surface these to the operator now
            // rather than letting a Pending row accumulate stale bad data.
            var validationErrors = CollectValidationErrors(dc, fromGstin, fromTradeName, toGstin, toTradeName,
                                                           transporterGstin, details);
            if (validationErrors.Count > 0)
            {
                return new ApiResponseDTO<GenerateEWaybillResponseDto>
                {
                    IsSuccess = false,
                    Message = "E-waybill cannot be generated due to missing data.",
                    Data = new GenerateEWaybillResponseDto
                    {
                        DeliveryNumber = dc.DeliveryNumber,
                        AlreadyExisted = false,
                        Errors = validationErrors
                    }
                };
            }

            // 6. Build Finance create command
            var createCmd = new CreateEWaybillHeaderCommand
            {
                UnitId = dc.FromPlantId,
                InvoiceNo = dc.DeliveryNumber,
                InvoiceDate = dc.DeliveryDate,
                InvoiceValue = dc.ConsignmentValue,
                TotalValue = dc.ConsignmentValue,
                SupplyType = SupplyTypeOutward,
                SubSupplyType = SubSupplyTypeForOwnUse,
                DocumentType = DocumentTypeDeliveryChallan,
                TransactionType = TransactionTypeRegular,
                FromGSTIN = fromGstin,
                FromTradeName = fromTradeName,
                ToGSTIN = toGstin,
                ToTradeName = toTradeName,
                CGST = 0,
                SGST = 0,
                IGST = 0,
                Cess = 0,
                TransporterId = dc.TransporterId > 0 ? dc.TransporterId : null,
                TransporterGSTIN = transporterGstin,
                TransporterName = transporterName,
                VehicleNo = dc.VehicleNumber,
                Distance = dc.TransportDistance.HasValue
                    ? (int?)Math.Round(dc.TransportDistance.Value, MidpointRounding.AwayFromZero)
                    : null,
                EwbStatus = EwbStatusPending,
                Details = details
            };

            var createResult = await _mediator.Send(createCmd, cancellationToken);
            if (!createResult.IsSuccess)
                throw new ExceptionRules($"Failed to create e-waybill for DC {dc.DeliveryNumber}: {createResult.Message}");

            var ewbHeaderId = createResult.Data;

            // 7. Call NIC standalone EWB API. The orchestrator command updates the row
            //    with the EWB number on success, or with error details on failure.
            var standalonePayload = BuildStandaloneEwbPayload(
                dc, fromGstin!, fromTradeName!, toGstin!, toTradeName!, transporterGstin, transporterName, details);

            var nicResult = await _mediator.Send(
                new GenerateStandaloneEwbCommand
                {
                    EWaybillHeaderId = ewbHeaderId,
                    Payload          = standalonePayload
                }, cancellationToken);

            // 8. Audit
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Generate",
                actionCode: "DELIVERYCHALLAN_GENERATE_EWAYBILL",
                actionName: dc.DeliveryNumber,
                details: $"E-waybill header {ewbHeaderId} for DC {dc.DeliveryNumber} — NIC result: {(nicResult.IsSuccess ? "Generated" : "Failed")}.",
                module: "DeliveryChallan");
            await _mediator.Publish(auditEvent, cancellationToken);

            // 9. Build response — surface NIC outcome to the UI
            if (nicResult.IsSuccess && nicResult.Data?.EwbNo is long ewbNo)
            {
                return new ApiResponseDTO<GenerateEWaybillResponseDto>
                {
                    IsSuccess = true,
                    Message   = $"E-waybill {ewbNo} generated for DC {dc.DeliveryNumber}.",
                    Data = new GenerateEWaybillResponseDto
                    {
                        EWaybillHeaderId = ewbHeaderId,
                        DeliveryNumber   = dc.DeliveryNumber,
                        EwbStatus        = "Generated",
                        EwbNumber        = ewbNo.ToString(CultureInfo.InvariantCulture),
                        AlreadyExisted   = false
                    }
                };
            }

            // NIC rejected — header row stays Pending with ErrorCode/ErrorMessage stamped.
            // Operator fixes the data and retries; idempotency picks up the same row.
            return new ApiResponseDTO<GenerateEWaybillResponseDto>
            {
                IsSuccess = false,
                Message   = nicResult.Data?.ErrorMessage ?? "NIC e-Waybill generation failed.",
                Data = new GenerateEWaybillResponseDto
                {
                    EWaybillHeaderId = ewbHeaderId,
                    DeliveryNumber   = dc.DeliveryNumber,
                    EwbStatus        = EwbStatusPending,
                    EwbNumber        = null,
                    AlreadyExisted   = false,
                    Errors = new List<string> { nicResult.Data?.ErrorMessage ?? "Unknown NIC error." }
                }
            };
        }

        // Maps DC + already-resolved GSTINs/items into the NIC standalone EWB payload.
        // FromUnitId / ToUnitId hint the service to enrich addresses from AppData.UnitAddress
        // so the DC handler doesn't need cross-schema SQL access.
        private static StandaloneEwbPayloadDto BuildStandaloneEwbPayload(
            DeliveryChallanHeaderDto dc,
            string fromGstin, string fromTradeName,
            string toGstin, string toTradeName,
            string? transporterGstin, string? transporterName,
            List<CreateEWaybillDetailDto> details)
        {
            var items = details.Select(d => new StandaloneEwbItemDto
            {
                ItemNo        = d.ItemSno,
                ProductName   = d.ItemName ?? string.Empty,
                ProductDesc   = d.ItemName,
                HsnCode       = int.TryParse(d.HsnNo, out var hsn) ? hsn : 0,
                Quantity      = d.Qty,
                QtyUnit       = string.IsNullOrWhiteSpace(d.UOM) ? "NOS" : d.UOM!,
                CgstRate      = 0,
                SgstRate      = 0,
                IgstRate      = 0,
                CessRate      = 0,
                CessNonAdvol  = 0,
                TaxableAmount = d.TaxableValue
            }).ToList();

            return new StandaloneEwbPayloadDto
            {
                SupplyType      = "O",                 // Outward
                SubSupplyType   = "5",                 // For Own Use (matches DC for inter-plant transfer)
                DocType         = "CHL",               // Delivery Challan
                DocNo           = dc.DeliveryNumber!,
                // NIC mandates dd/MM/yyyy with literal slashes — InvariantCulture avoids
                // locale-driven separator substitution (e.g., en-IN renders as dd-MM-yyyy).
                DocDate         = dc.DeliveryDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                TransactionType = 1,                   // Regular

                FromGstin   = fromGstin,
                FromTrdName = fromTradeName,
                FromUnitId  = dc.FromPlantId,          // Service will enrich addr/place/pincode

                ToGstin   = toGstin,
                ToTrdName = toTradeName,
                ToUnitId  = dc.ToPlantId,

                TotalValue        = dc.ConsignmentValue,
                TotInvValue       = dc.ConsignmentValue,
                CgstValue         = 0,
                SgstValue         = 0,
                IgstValue         = 0,
                CessValue         = 0,
                CessNonAdvolValue = 0,
                OtherValue        = 0,

                TransMode        = "1",                // Road
                TransDistance    = dc.TransportDistance.HasValue
                    ? (int)Math.Round(dc.TransportDistance.Value, MidpointRounding.AwayFromZero)
                    : 0,
                TransporterName  = transporterName,
                TransporterId    = transporterGstin,
                VehicleNo        = dc.VehicleNumber,
                VehicleType      = "R",                // Regular

                ItemList = items
            };
        }

        /// <summary>
        /// Collects every blocking master-data gap that would cause NIC to reject the EWB.
        /// Returns an empty list when the EWB is safe to insert.
        /// </summary>
        private static List<string> CollectValidationErrors(
            DeliveryChallanHeaderDto dc,
            string? fromGstin, string? fromTradeName,
            string? toGstin, string? toTradeName,
            string? transporterGstin,
            List<CreateEWaybillDetailDto> details)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(fromGstin))
                errors.Add($"Consignor GSTIN missing on Unit {dc.FromPlantId}. Update the Company linked to that Unit.");
            if (string.IsNullOrWhiteSpace(fromTradeName))
                errors.Add($"Consignor trade name missing on Unit {dc.FromPlantId}.");

            if (string.IsNullOrWhiteSpace(toGstin))
                errors.Add($"Consignee GSTIN missing on Unit {dc.ToPlantId}. Update the Company linked to that Unit.");
            if (string.IsNullOrWhiteSpace(toTradeName))
                errors.Add($"Consignee trade name missing on Unit {dc.ToPlantId}.");

            if (dc.TransporterId > 0 && string.IsNullOrWhiteSpace(transporterGstin))
                errors.Add($"Transporter GSTIN missing on Party {dc.TransporterId}.");

            if (string.IsNullOrWhiteSpace(dc.VehicleNumber))
                errors.Add("Vehicle number missing on Delivery Challan.");

            if (!dc.TransportDistance.HasValue || dc.TransportDistance.Value <= 0)
                errors.Add("Transport distance missing or zero on Delivery Challan.");

            if (details.Count == 0)
            {
                errors.Add("Delivery Challan has no line items.");
            }
            else
            {
                foreach (var line in details)
                {
                    if (string.IsNullOrWhiteSpace(line.ItemName))
                        errors.Add($"Item name missing for ItemId {line.ItemId} (line {line.ItemSno}).");
                    if (string.IsNullOrWhiteSpace(line.HsnNo))
                        errors.Add($"HSN number missing for ItemId {line.ItemId} (line {line.ItemSno}).");
                }
            }

            return errors;
        }

        private async Task<(string? gstin, string? tradeName)> ResolvePlantGstinAsync(int plantId)
        {
            if (plantId <= 0) return (null, null);

            var unit = await _unitLookup.GetByIdAsync(plantId);
            if (unit == null || unit.CompanyId <= 0) return (null, null);

            var companies = await _companyLookup.GetAllCompanyAsync();
            var company = companies.FirstOrDefault(c => c.CompanyId == unit.CompanyId);
            if (company == null) return (null, null);

            return (company.GstNumber, company.LegalName);
        }

        /// <summary>
        /// Maps DC detail rows into EWaybill detail DTOs, enriched with HSN/ItemName/UOM
        /// from cross-module lookups. DCs carry no tax, so CGST/SGST/IGST/Cess stay 0.
        /// </summary>
        private async Task<List<CreateEWaybillDetailDto>> BuildEWaybillDetailsAsync(
            DeliveryChallanHeaderDto dc, CancellationToken ct)
        {
            var dcDetails = dc.DeliveryChallanDetails;
            if (dcDetails == null || dcDetails.Count == 0)
                return new List<CreateEWaybillDetailDto>();

            // Batch-fetch master data for all lines in one round trip each
            var itemIds = dcDetails.Select(d => d.ItemId).Where(i => i > 0).Distinct().ToList();
            var uomIds  = dcDetails.Select(d => d.UOMId).Where(i => i > 0).Distinct().ToList();

            var items = itemIds.Count > 0
                ? await _itemLookup.GetByIdsAsync(itemIds, ct)
                : new List<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>();
            var uoms = uomIds.Count > 0
                ? await _uomLookup.GetByIdsAsync(uomIds, ct)
                : new List<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>();

            var itemDict = items.ToDictionary(i => i.Id);
            var uomDict  = uoms .ToDictionary(u => u.Id);

            var lines = new List<CreateEWaybillDetailDto>(dcDetails.Count);
            int sno = 1;
            foreach (var d in dcDetails)
            {
                itemDict.TryGetValue(d.ItemId, out var item);
                uomDict.TryGetValue(d.UOMId, out var uom);

                lines.Add(new CreateEWaybillDetailDto
                {
                    ItemSno      = sno++,
                    ItemId       = d.ItemId,
                    ItemName     = item?.ItemName ?? d.ItemName,
                    HsnNo        = item?.HSNCode,
                    IsService    = "N",                                     // DC = goods
                    Qty          = d.DispatchQuantity,
                    UOM          = uom?.Code ?? uom?.UOMName ?? d.UOMName,
                    TaxableValue = d.LineMovementValue,
                    TaxRate      = 0,
                    CGST         = 0,
                    SGST         = 0,
                    IGST         = 0,
                    Cess         = 0
                });
            }

            return lines;
        }
    }
}
