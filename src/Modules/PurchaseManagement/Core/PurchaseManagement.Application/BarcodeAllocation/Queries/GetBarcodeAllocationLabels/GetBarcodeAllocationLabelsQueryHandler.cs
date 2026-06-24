using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationLabels
{
    public class GetBarcodeAllocationLabelsQueryHandler
        : IRequestHandler<GetBarcodeAllocationLabelsQuery, BarcodeLabelReportDto?>
    {
        // Safety guard: never expand an unbounded range into one response.
        private const int MaxLabels = 5000;

        private readonly IBarcodeAllocationQueryRepository _queryRepository;
        private readonly ICompanyDetailLookup _companyDetail;
        private readonly IDivisionUnitLookup _divisionLookup;
        private readonly ICityLookup _cityLookup;
        private readonly IStateLookup _stateLookup;
        private readonly IIPAddressService _ipService;
        private readonly IMediator _mediator;

        public GetBarcodeAllocationLabelsQueryHandler(
            IBarcodeAllocationQueryRepository queryRepository,
            ICompanyDetailLookup companyDetail,
            IDivisionUnitLookup divisionLookup,
            ICityLookup cityLookup,
            IStateLookup stateLookup,
            IIPAddressService ipService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _companyDetail = companyDetail;
            _divisionLookup = divisionLookup;
            _cityLookup = cityLookup;
            _stateLookup = stateLookup;
            _ipService = ipService;
            _mediator = mediator;
        }

        public async Task<BarcodeLabelReportDto?> Handle(GetBarcodeAllocationLabelsQuery request, CancellationToken cancellationToken)
        {
            var allocation = await _queryRepository.GetByIdAsync(request.Id);
            if (allocation == null)
                return null;

            // Letterhead — resolved from the logged-in company / division / unit (no schema link on the pool).
            var companyId = _ipService.GetCompanyId() ?? 0;
            var divisionId = _ipService.GetDivisionId() ?? 0;
            var unitId = _ipService.GetUnitId() ?? 0;

            var company = await _companyDetail.GetByUnitIdAsync(unitId, cancellationToken);

            var divisions = await _divisionLookup.GetUnitsByDivisionAsync(companyId, divisionId, cancellationToken);
            var divisionName = divisions?.FirstOrDefault(d => d.UnitId == unitId)?.DivisionName
                               ?? divisions?.FirstOrDefault()?.DivisionName;

            string? cityName = null, stateName = null;
            if (company != null)
            {
                if (company.CityId > 0)
                    cityName = (await _cityLookup.GetByIdAsync(company.CityId, cancellationToken))?.CityName;
                if (company.StateId > 0)
                    stateName = (await _stateLookup.GetByIdAsync(company.StateId, cancellationToken))?.StateName;
            }

            // Expand the range into individual barcodes (prefix + number), capped for safety.
            long from = allocation.BarcodeFrom;
            long to = allocation.BarcodeTo;
            long fullCount = to >= from ? to - from + 1 : 0;

            var labels = new List<BarcodeLabelItemDto>();
            for (long n = from; n <= to && labels.Count < MaxLabels; n++)
            {
                var code = $"{allocation.Prefix}{n}";
                labels.Add(new BarcodeLabelItemDto { Barcode = code, QrPayload = code });
            }

            var report = new BarcodeLabelReportDto
            {
                Letterhead = new BarcodeLetterheadDto
                {
                    CompanyName = company?.CompanyName,
                    DivisionName = divisionName,
                    Address = BuildAddress(company?.AddressLine1, company?.AddressLine2, cityName, company?.PinCode, stateName)
                },
                AllocationNumber = allocation.AllocationNumber,
                SeriesNumber = allocation.BarcodeSeriesNumber,
                Prefix = allocation.Prefix,
                TotalCount = fullCount,
                Truncated = fullCount > MaxLabels,
                Labels = labels
            };

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetLabels",
                actionCode: "GetBarcodeAllocationLabelsQuery",
                actionName: allocation.Id.ToString(),
                details: $"Barcode allocation labels {allocation.Id} were generated.",
                module: "BarcodeAllocation");
            await _mediator.Publish(domainEvent, cancellationToken);

            return report;
        }

        // "Trichy Road NH-45, DINDIGUL - 624 802, Tamil Nadu"
        private static string BuildAddress(string? line1, string? line2, string? city, string? pinCode, string? state)
        {
            var parts = new List<string>();
            foreach (var s in new[] { line1, line2, city })
            {
                if (!string.IsNullOrWhiteSpace(s))
                    parts.Add(s!.Trim());
            }

            var address = string.Join(", ", parts);

            if (!string.IsNullOrWhiteSpace(pinCode))
                address = address.Length > 0 ? $"{address} - {pinCode}" : pinCode!;

            if (!string.IsNullOrWhiteSpace(state))
                address = address.Length > 0 ? $"{address}, {state}" : state!;

            return address;
        }
    }
}
