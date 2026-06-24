using System.Globalization;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCREntryReport
{
    public class GetOCREntryReportQueryHandler : IRequestHandler<GetOCREntryReportQuery, OcrReportDto?>
    {
        private readonly IOCREntryQueryRepository _queryRepository;
        private readonly ICompanyDetailLookup _companyDetail;
        private readonly IUnitDetailLookup _unitDetail;
        private readonly ICityLookup _cityLookup;
        private readonly IStateLookup _stateLookup;
        private readonly IWorkflowLookup _workflow;
        private readonly IIPAddressService _ipService;
        private readonly IMediator _mediator;

        public GetOCREntryReportQueryHandler(
            IOCREntryQueryRepository queryRepository,
            ICompanyDetailLookup companyDetail,
            IUnitDetailLookup unitDetail,
            ICityLookup cityLookup,
            IStateLookup stateLookup,
            IWorkflowLookup workflow,
            IIPAddressService ipService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _companyDetail = companyDetail;
            _unitDetail = unitDetail;
            _cityLookup = cityLookup;
            _stateLookup = stateLookup;
            _workflow = workflow;
            _ipService = ipService;
            _mediator = mediator;
        }

        public async Task<OcrReportDto?> Handle(GetOCREntryReportQuery request, CancellationToken cancellationToken)
        {
            var ocr = await _queryRepository.GetByIdAsync(request.Id);
            if (ocr == null)
                return null;

            // Letterhead — resolved from the logged-in unit (matches the OCR document-URL convention).
            var unitId = _ipService.GetUnitId() ?? 0;
            var company = await _companyDetail.GetByUnitIdAsync(unitId, cancellationToken);
            var unit = await _unitDetail.GetByIdAsync(unitId, cancellationToken);

            string? cityName = null, stateName = null;
            if (unit != null)
            {
                if (unit.CityId > 0)
                    cityName = (await _cityLookup.GetByIdAsync(unit.CityId, cancellationToken))?.CityName;
                if (unit.StateId > 0)
                    stateName = (await _stateLookup.GetByIdAsync(unit.StateId, cancellationToken))?.StateName;
            }

            // Cotton Approved (by / on) — final approval, cross-module workflow lookup.
            var approval = await _workflow.GetApprovalInfoAsync(
                MiscEnumEntity.TransactionTypeOCR, ocr.Id, cancellationToken);

            // Freight (per bale / total) — via OCR → PO → Freight RFQ; null until approved.
            var (freightPerBale, freightTotal) = await _queryRepository.GetFreightForOcrAsync(ocr.Id);

            var report = new OcrReportDto
            {
                Sections = new List<OcrReportSection>
                {
                    new()
                    {
                        Key = "company", Title = "Company",
                        Fields = new List<OcrReportField>
                        {
                            Field("companyName", "Company", company?.CompanyName),
                            Field("unit", "Unit", unit?.UnitName),
                            Field("address", "Address", BuildAddress(unit?.AddressLine1, unit?.AddressLine2, cityName, unit?.PinCode, stateName)),
                            Field("cin", "CIN", unit?.CINNO),
                            Field("gstin", "GSTIN", company?.GstNumber),
                            Field("logo", "Logo", company?.LogoUrl),
                        }
                    },
                    new()
                    {
                        Key = "documentIdentity", Title = "Document Identity",
                        Fields = new List<OcrReportField>
                        {
                            Field("ocrNumber", "OCR No.", ocr.OcrNumber),
                            Field("ocrDate", "Date", FmtDate(ocr.OcrDate), ocr.OcrDate),
                        }
                    },
                    new()
                    {
                        Key = "orderDetails", Title = "Order Details",
                        Fields = new List<OcrReportField>
                        {
                            Field("sellerName", "Name of Seller", ocr.SupplierName),
                            Field("station", "Station", ocr.StationName),
                            Field("agentName", "Name of Agent", ocr.BrokerName),
                            Field("goods", "Description of Goods", ocr.ItemName),
                            Field("quantity", "Quantity", $"{FmtNumber(ocr.Quantity)} Bales", ocr.Quantity),
                            Field("rateCandy", $"Rate/{(string.IsNullOrWhiteSpace(ocr.UomName) ? "Candy" : ocr.UomName)}", BuildRateCandy(ocr.Rate, ocr.GstPercentage, ocr.ProcurementTypeName), ocr.Rate),
                            Field("deliveryPeriod", "Delivery Period (Tentative)", FmtDate(ocr.ExpectedDispatchDate), ocr.ExpectedDispatchDate),
                            Field("modeOfPayment", "Mode Of Payment", ocr.PaymentTermName),
                            Field("transitInsurance", "Transit Insurance", ocr.TransitInsuranceName),
                            Field("lorryFreight", "Lorry Freight", ocr.LorryFreightName),
                            Field("cottonPassedBy", "Cotton Passed By", ocr.CottonPassedBy),
                            Field("remarks", "Remarks If Any", ocr.Remarks),
                            Field("cottonApprovedBy", "Cotton Approved (by)", approval?.ApproverName),
                            Field("cottonApprovedOn", "Cotton Approved (on)", FmtDate(approval?.ApprovedDate), approval?.ApprovedDate),
                            Field("weight", "Weight", ocr.WeighmentName),
                        }
                    },
                    new()
                    {
                        Key = "cottonParameters", Title = "Cotton Parameters",
                        Fields = BuildCottonParameters(ocr)
                    },
                    new()
                    {
                        Key = "freight", Title = "Freight",
                        Fields = new List<OcrReportField>
                        {
                            Field("freightPerBale", "Freight Amount (Per Bale)", FmtMoney(freightPerBale), freightPerBale),
                            Field("freightTotal", "Freight Amount (Total)", FmtMoney(freightTotal), freightTotal),
                        }
                    },
                    new()
                    {
                        Key = "footer", Title = "Signatories",
                        Fields = new List<OcrReportField>
                        {
                            Field("cottonDepartment", "Cotton Department", null),
                            Field("cfo", "Chief Financial Officer", null),
                            Field("md", "MD", null),
                        }
                    },
                }
            };

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetReport",
                actionCode: "GetOCREntryReportQuery",
                actionName: ocr.Id.ToString(),
                details: $"OCR report {ocr.Id} was generated.",
                module: "OCREntry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return report;
        }

        // Cotton-quality parameters are dynamic (driven by the OCR's quality template); only the
        // rows that actually exist are emitted, followed by the always-present Bale Grade field.
        private static List<OcrReportField> BuildCottonParameters(OCREntryDto ocr)
        {
            var fields = new List<OcrReportField>();

            foreach (var p in ocr.QualityParameters)
            {
                var key = string.IsNullOrWhiteSpace(p.ParameterCode) ? $"param{p.ParamId}" : p.ParameterCode!;
                var label = string.IsNullOrWhiteSpace(p.ParameterName) ? key : p.ParameterName!;
                fields.Add(Field(key, label, p.Value));
            }

            fields.Add(Field("baleGrade", "BALE GRADE", ocr.GradeName));
            return fields;
        }

        private static OcrReportField Field(string key, string label, string? value, object? raw = null) =>
            new() { Key = key, Label = label, Value = value ?? string.Empty, Raw = raw };

        private static string FmtDate(DateTimeOffset? d) =>
            d.HasValue ? d.Value.ToString("dd.MM.yy", CultureInfo.InvariantCulture) : string.Empty;

        private static string FmtNumber(decimal value) =>
            value.ToString("#,##0.###", CultureInfo.InvariantCulture);

        private static string FmtMoney(decimal? value) =>
            value.HasValue ? $"Rs.{value.Value.ToString("#,##0.00", CultureInfo.InvariantCulture)}" : string.Empty;

        // "Rs.365.00 + 5.00% GST Spot" — rate + optional GST% + optional procurement type.
        private static string BuildRateCandy(decimal rate, decimal? gstPercentage, string? procurementTypeName)
        {
            var text = $"Rs.{rate.ToString("#,##0.00", CultureInfo.InvariantCulture)}";

            if (gstPercentage.HasValue && gstPercentage.Value > 0)
                text += $" + {gstPercentage.Value.ToString("0.00", CultureInfo.InvariantCulture)}% GST";

            if (!string.IsNullOrWhiteSpace(procurementTypeName))
                text += $" {procurementTypeName}";

            return text;
        }

        // "252, METTUPALAYAM ROAD, COIMBATORE - 641043, Tamil Nadu"
        private static string BuildAddress(string? line1, string? line2, string? city, int? pinCode, string? state)
        {
            var parts = new List<string>();
            foreach (var s in new[] { line1, line2, city })
            {
                if (!string.IsNullOrWhiteSpace(s))
                    parts.Add(s!.Trim());
            }

            var address = string.Join(", ", parts);

            if (pinCode.HasValue && pinCode.Value > 0)
                address = address.Length > 0 ? $"{address} - {pinCode.Value}" : pinCode.Value.ToString();

            if (!string.IsNullOrWhiteSpace(state))
                address = address.Length > 0 ? $"{address}, {state}" : state!;

            return address;
        }
    }
}
