using Contracts.Common;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader;
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

            // 6. Audit
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Generate",
                actionCode: "DELIVERYCHALLAN_GENERATE_EWAYBILL",
                actionName: dc.DeliveryNumber,
                details: $"E-waybill header {createResult.Data} generated for Delivery Challan {dc.DeliveryNumber} (Id {dc.Id}).",
                module: "DeliveryChallan");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<GenerateEWaybillResponseDto>
            {
                IsSuccess = true,
                Message = $"E-waybill generated for DC {dc.DeliveryNumber}.",
                Data = new GenerateEWaybillResponseDto
                {
                    EWaybillHeaderId = createResult.Data,
                    DeliveryNumber = dc.DeliveryNumber,
                    EwbStatus = EwbStatusPending,
                    EwbNumber = null,
                    AlreadyExisted = false
                }
            };
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
