using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;
using PurchaseManagement.Application.PurchaseOrder.Print.Dto;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.Print;

internal sealed class PurchaseOrderPrintQueryRepository : IPurchaseOrderPrintQueryRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly IIPAddressService _ipAddressService;
    private readonly ICompanyDetailLookup _companyDetailLookup;
    private readonly IUnitDetailLookup _unitDetailLookup;
    private readonly IPartyDetailLookup _partyDetailLookup;
    private readonly IPartyBankLookup _partyBankLookup;
    private readonly IStateLookup _stateLookup;
    private readonly ICityLookup _cityLookup;
    private readonly IItemLookup _itemLookup;
    private readonly IUOMLookup _uomLookup;
    private readonly ICurrencyLookup _currencyLookup;
    private readonly IDepartmentLookup _departmentLookup;
    private readonly ICountryLookup _countryLookup;
    private readonly ITransactionTypeLookup _transactionTypeLookup;

    public PurchaseOrderPrintQueryRepository(
        IDbConnection dbConnection,
        IIPAddressService ipAddressService,
        ICompanyDetailLookup companyDetailLookup,
        IUnitDetailLookup unitDetailLookup,
        IPartyDetailLookup partyDetailLookup,
        IPartyBankLookup partyBankLookup,
        IStateLookup stateLookup,
        ICityLookup cityLookup,
        IItemLookup itemLookup,
        IUOMLookup uomLookup,
        ICurrencyLookup currencyLookup,
        IDepartmentLookup departmentLookup,
        ICountryLookup countryLookup,
        ITransactionTypeLookup transactionTypeLookup)
    {
        _dbConnection = dbConnection;
        _ipAddressService = ipAddressService;
        _companyDetailLookup = companyDetailLookup;
        _unitDetailLookup = unitDetailLookup;
        _partyDetailLookup = partyDetailLookup;
        _partyBankLookup = partyBankLookup;
        _stateLookup = stateLookup;
        _cityLookup = cityLookup;
        _itemLookup = itemLookup;
        _uomLookup = uomLookup;
        _currencyLookup = currencyLookup;
        _departmentLookup = departmentLookup;
        _countryLookup = countryLookup;
        _transactionTypeLookup = transactionTypeLookup;
    }

    public async Task<PurchaseOrderPrintDto?> GetPrintDetailsAsync(int purchaseOrderId, CancellationToken ct = default)
    {
        var unitId = _ipAddressService.GetUnitId();

        // ── Step 1: Fetch PO Header with same-module JOINs ──
        const string headerSql = @"
            SELECT h.Id, h.UnitId, h.PONumber, h.PODate, h.POCategoryId,
                   mCat.Code AS POCategoryCode, mCat.Description AS POCategoryDescription,
                   h.POMethodId, mMethod.Code AS POMethodCode, mMethod.Description AS POMethodDescription,
                   h.CurrencyId, h.VendorId,
                   h.ItemTotal, h.DiscountTotal, h.PandFTotal, h.MiscCharges,
                   h.GSTTotal, h.CGSTTotal, h.SGSTTotal, h.IGSTTotal,
                   h.FreightTotal, h.InsuranceTotal, h.TDSTotal, h.AdvanceAmount,
                   h.PurchaseValue, h.StatusId, mStatus.Description AS StatusDescription,
                   h.RevisionNo, h.AmendmentReason
            FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster mCat ON mCat.Id = h.POCategoryId AND mCat.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mMethod ON mMethod.Id = h.POMethodId AND mMethod.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mStatus ON mStatus.Id = h.StatusId AND mStatus.IsDeleted = 0
            WHERE h.Id = @Id AND h.IsDeleted = 0 AND h.UnitId = @UnitId";

        var header = await _dbConnection.QueryFirstOrDefaultAsync<POHeaderRawDto>(
            headerSql, new { Id = purchaseOrderId, UnitId = unitId });

        if (header == null)
            return null;

        // ── Step 2: Determine PO type ──
        string poType;
        if (string.Equals(header.POCategoryCode, MiscEnumEntity.POCategoryService, StringComparison.OrdinalIgnoreCase))
            poType = "Service";
        else
            poType = header.POMethodCode switch
            {
                MiscEnumEntity.Import => "Import",
                MiscEnumEntity.Contract => "Contract",
                _ => "Local"
            };

        // ── Step 3: Fetch type-specific header and details ──
        LocalHeaderRawDto? localHeader = null;
        List<LocalDetailRawDto>? localDetails = null;
        ContractHeaderRawDto? contractHeader = null;
        List<ContractDetailRawDto>? contractDetails = null;
        ImportHeaderRawDto? importHeader = null;
        List<ImportDetailRawDto>? importDetails = null;
        ServiceHeaderRawDto? serviceHeader = null;
        List<ServiceLineRawDto>? serviceLines = null;

        switch (poType)
        {
            case "Local":
                (localHeader, localDetails) = await FetchLocalDataAsync(purchaseOrderId);
                break;
            case "Contract":
                (contractHeader, contractDetails) = await FetchContractDataAsync(purchaseOrderId);
                break;
            case "Import":
                (importHeader, importDetails) = await FetchImportDataAsync(purchaseOrderId);
                break;
            case "Service":
                (serviceHeader, serviceLines) = await FetchServiceDataAsync(purchaseOrderId);
                break;
        }

        // ── Step 4: Fetch payment terms ──
        const string paymentTermSql = @"
            SELECT mPT.Description AS PaymentTermName,
                   pt.AdvancePercent, pt.CreditDays,
                   mPM.Description AS PaymentModeName,
                   pt.InsuranceAmount, pt.AdvanceAmount, pt.BalanceAmount
            FROM Purchase.PurchasePaymentTerm pt WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster mPT ON mPT.Id = pt.PaymentTermId AND mPT.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mPM ON mPM.Id = pt.PaymentModelId AND mPM.IsDeleted = 0
            WHERE pt.PurchaseOrderId = @PurchaseOrderId AND pt.IsDeleted = 0";

        var paymentTerms = (await _dbConnection.QueryAsync<PaymentTermRawDto>(
            paymentTermSql, new { PurchaseOrderId = purchaseOrderId })).ToList();

        // ── Step 5: Cross-module lookups ──
        var company = await _companyDetailLookup.GetByUnitIdAsync(header.UnitId, ct);
        var unit = await _unitDetailLookup.GetByIdAsync(header.UnitId, ct);
        var vendor = await _partyDetailLookup.GetByIdAsync(header.VendorId, ct);

        // Currency
        var currencies = await _currencyLookup.GetByIdsAsync(new[] { header.CurrencyId }, ct);
        var currency = currencies.FirstOrDefault();

        // Collect item IDs and UOM IDs for batch lookup
        var itemIds = new HashSet<int>();
        var uomIds = new HashSet<int>();
        var deptIds = new HashSet<int>();

        if (localDetails != null)
        {
            foreach (var d in localDetails)
            {
                itemIds.Add(d.ItemId);
                uomIds.Add(d.UOMId);
                if (d.DepartmentId.HasValue) deptIds.Add(d.DepartmentId.Value);
            }
        }
        if (contractDetails != null)
        {
            foreach (var d in contractDetails)
            {
                itemIds.Add(d.ItemId);
                uomIds.Add(d.UOMId);
                if (d.DepartmentId.HasValue) deptIds.Add(d.DepartmentId.Value);
            }
        }
        if (importDetails != null)
        {
            foreach (var d in importDetails)
            {
                itemIds.Add(d.ItemId);
                uomIds.Add(d.UomId);
            }
        }
        if (serviceLines != null)
        {
            foreach (var d in serviceLines)
            {
                if (d.UOMId.HasValue) uomIds.Add(d.UOMId.Value);
            }
        }

        var items = itemIds.Count > 0
            ? (await _itemLookup.GetByIdsAsync(itemIds, ct)).ToDictionary(i => i.Id)
            : new Dictionary<int, ItemLookupDto>();

        var uoms = uomIds.Count > 0
            ? (await _uomLookup.GetByIdsAsync(uomIds, ct)).ToDictionary(u => u.Id)
            : new Dictionary<int, UOMLookupDto>();

        var depts = deptIds.Count > 0
            ? (await _departmentLookup.GetByIdsAsync(deptIds, ct)).ToDictionary(d => d.DepartmentId)
            : new Dictionary<int, DepartmentLookupDto>();

        // State/City lookups for company, unit, vendor addresses
        var stateIds = new HashSet<int>();
        var cityIds = new HashSet<int>();

        if (company != null)
        {
            if (company.StateId > 0) stateIds.Add(company.StateId);
            if (company.CityId > 0) cityIds.Add(company.CityId);
        }
        if (unit != null)
        {
            if (unit.StateId > 0) stateIds.Add(unit.StateId);
            if (unit.CityId > 0) cityIds.Add(unit.CityId);
        }
        if (vendor != null)
        {
            if (vendor.StateId > 0) stateIds.Add(vendor.StateId);
            if (vendor.CityId > 0) cityIds.Add(vendor.CityId);
        }

        var states = stateIds.Count > 0
            ? (await _stateLookup.GetByIdsAsync(stateIds, ct)).ToDictionary(s => s.StateId, s => s.StateName)
            : new Dictionary<int, string>();

        var cities = cityIds.Count > 0
            ? (await _cityLookup.GetByIdsAsync(cityIds, ct)).ToDictionary(c => c.CityId, c => c.CityName)
            : new Dictionary<int, string>();

        // Vendor bank
        var bank = vendor?.GSTNumber != null
            ? await _partyBankLookup.GetDefaultBankByGstAsync(vendor.GSTNumber, ct)
            : null;

        // Import-specific: origin country
        string? originCountryName = null;
        if (poType == "Import" && importHeader?.OriginCountryId.HasValue == true)
        {
            var country = await _countryLookup.GetByIdAsync(importHeader.OriginCountryId.Value, ct);
            originCountryName = country?.CountryName;
        }

        // ── Step 6: Terms & Conditions (matched by the PO's transaction type) ──
        var termsHtml = await FetchTermsHtmlAsync(poType, ct);

        // ── Step 7: Assemble DTO ──
        var dto = AssembleDto(
            poType, header,
            localHeader, localDetails,
            contractHeader, contractDetails,
            importHeader, importDetails,
            serviceHeader, serviceLines,
            paymentTerms,
            company, unit, vendor, currency, bank,
            items, uoms, depts, states, cities,
            originCountryName);

        dto.TermsHtml = termsHtml;
        return dto;
    }

    // Resolves the active T&C template's TermsHtml for this PO by mapping the PO type to a
    // Finance transaction type (name → id via lookup, no cross-module JOIN), then matching
    // the same-module TnCTemplateApplicability.
    private async Task<string?> FetchTermsHtmlAsync(string poType, CancellationToken ct)
    {
        var transactionTypeName = poType switch
        {
            "Local" => MiscEnumEntity.TransactionTypeLPO,
            "Import" => MiscEnumEntity.TransactionTypeIPO,
            "Contract" => MiscEnumEntity.TransactionTypeCPO,
            "Service" => MiscEnumEntity.TransactionTypeSPO,
            "Blanket" => MiscEnumEntity.TransactionTypeBPO,
            _ => null
        };
        if (transactionTypeName == null) return null;

        var transactionTypes = await _transactionTypeLookup.GetAllTransactionTypeAsync();
        var transactionType = transactionTypes.FirstOrDefault(t =>
            string.Equals(t.TypeName, transactionTypeName, StringComparison.OrdinalIgnoreCase));
        if (transactionType == null) return null;

        const string sql = @"
            SELECT TOP 1 t.TermsHtml
            FROM Purchase.TnCTemplateMaster t WITH (NOLOCK)
            INNER JOIN Purchase.TnCTemplateApplicability a WITH (NOLOCK)
                   ON a.TnCTemplateMasterId = t.Id AND a.IsDeleted = 0 AND a.IsActive = 1
            WHERE a.TransactionTypeId = @TransactionTypeId
              AND t.IsDeleted = 0 AND t.IsActive = 1
            ORDER BY t.ModifiedDate DESC, t.CreatedDate DESC, t.Id DESC";

        return await _dbConnection.ExecuteScalarAsync<string?>(
            new CommandDefinition(sql, new { TransactionTypeId = transactionType.Id }, cancellationToken: ct));
    }

    // ──────────────────── Type-Specific Data Fetchers ────────────────────

    private async Task<(LocalHeaderRawDto?, List<LocalDetailRawDto>?)> FetchLocalDataAsync(int poId)
    {
        const string headerSql = @"
            SELECT lh.Id, lh.IsPartialReceiptAllowed,
                   mInco.Description AS IncotermsName,
                   mMod.Description AS ModeOfDispatchName,
                   lh.FreightCharges, lh.TermDescription,
                   lh.DeliveryAddress, lh.BillingAddress
            FROM Purchase.PurchaseLocalHeader lh WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster mInco ON mInco.Id = lh.IncotermsId AND mInco.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mMod ON mMod.Id = lh.ModeOfDispatchId AND mMod.IsDeleted = 0
            WHERE lh.PurchaseOrderId = @PoId AND lh.IsDeleted = 0";

        var lh = await _dbConnection.QueryFirstOrDefaultAsync<LocalHeaderRawDto>(headerSql, new { PoId = poId });
        if (lh == null) return (null, null);

        const string detailSql = @"
            SELECT d.ItemSno, d.ItemId, d.UOMId, d.Quantity, d.UnitPrice, d.ItemValue,
                   d.DiscountValue, d.GSTPercentage, d.CGSTPercentage, d.SGSTPercentage,
                   d.IGSTPercentage, d.CGST, d.SGST, d.IGST, d.ScheduleDate, d.DepartmentId
            FROM Purchase.PurchaseLocalDetail d WITH (NOLOCK)
            WHERE d.PurchaseLocalId = @HeaderId
            ORDER BY d.ItemSno";

        var details = (await _dbConnection.QueryAsync<LocalDetailRawDto>(detailSql, new { HeaderId = lh.Id })).ToList();
        return (lh, details);
    }

    private async Task<(ContractHeaderRawDto?, List<ContractDetailRawDto>?)> FetchContractDataAsync(int poId)
    {
        const string headerSql = @"
            SELECT ch.Id, ch.ContractPOHeaderId,
                   cph.ContractPONo AS ContractPONumber,
                   ch.IsPartialReceiptAllowed,
                   mInco.Description AS IncotermsName,
                   mMod.Description AS ModeOfDispatchName,
                   ch.FreightCharges, ch.TermDescription,
                   ch.DeliveryAddress, ch.BillingAddress
            FROM Purchase.PurchaseContractHeader ch WITH (NOLOCK)
            LEFT JOIN Purchase.ContractPOHeader cph ON cph.Id = ch.ContractPOHeaderId AND cph.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mInco ON mInco.Id = ch.IncotermsId AND mInco.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mMod ON mMod.Id = ch.ModeOfDispatchId AND mMod.IsDeleted = 0
            WHERE ch.PurchaseOrderId = @PoId AND ch.IsDeleted = 0";

        var ch = await _dbConnection.QueryFirstOrDefaultAsync<ContractHeaderRawDto>(headerSql, new { PoId = poId });
        if (ch == null) return (null, null);

        const string detailSql = @"
            SELECT d.ItemSno, d.ItemId, d.UOMId, d.Quantity, d.UnitPrice, d.ItemValue,
                   d.DiscountValue, d.GSTPercentage, d.CGSTPercentage, d.SGSTPercentage,
                   d.IGSTPercentage, d.CGST, d.SGST, d.IGST, d.ScheduleDate, d.DepartmentId
            FROM Purchase.PurchaseContractDetail d WITH (NOLOCK)
            WHERE d.PurchaseContractHeaderId = @HeaderId
            ORDER BY d.ItemSno";

        var details = (await _dbConnection.QueryAsync<ContractDetailRawDto>(detailSql, new { HeaderId = ch.Id })).ToList();
        return (ch, details);
    }

    private async Task<(ImportHeaderRawDto?, List<ImportDetailRawDto>?)> FetchImportDataAsync(int poId)
    {
        const string headerSql = @"
            SELECT ih.Id, ih.TTExchangeRate,
                   mInco.Description AS IncotermsName,
                   sp.PortName AS ShippingPortName,
                   dp.PortName AS DestinationPortName,
                   mMot.Description AS ModeOfTransportName,
                   mSm.Description AS ShippingModeName,
                   ih.OriginCountryId,
                   ih.BillOfLadingNumber, ih.VesselName, ih.ContainerNumber,
                   ih.ExpectedTimeOfDeparture, ih.ExpectedTimeOfArrival,
                   ih.LCNumber, ih.LCDate, ih.LCAmount,
                   ih.TTReferenceNumber, ih.TTTransferDate,
                   ih.IsPartialReceiptAllowed
            FROM Purchase.PurchaseOrderImportHeader ih WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster mInco ON mInco.Id = ih.IncotermId AND mInco.IsDeleted = 0
            LEFT JOIN Purchase.PortMaster sp ON sp.Id = ih.ShippingPortId AND sp.IsDeleted = 0
            LEFT JOIN Purchase.PortMaster dp ON dp.Id = ih.DestinationPortId AND dp.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mMot ON mMot.Id = ih.ModeOfTransportId AND mMot.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mSm ON mSm.Id = ih.ShippingModeId AND mSm.IsDeleted = 0
            WHERE ih.PurchaseOrderId = @PoId AND ih.IsDeleted = 0";

        var ih = await _dbConnection.QueryFirstOrDefaultAsync<ImportHeaderRawDto>(headerSql, new { PoId = poId });
        if (ih == null) return (null, null);

        const string detailSql = @"
            SELECT d.ItemSno, d.ItemId, d.UomId, d.Quantity, d.UnitPrice,
                   d.FreightAmount, d.InsuranceAmount, d.CIFValue,
                   d.BasicCustomDuty, d.IGSTPercentage, d.IGST,
                   d.OtherCharges, d.TotalValue
            FROM Purchase.PurchaseOrderImportDetail d WITH (NOLOCK)
            WHERE d.PurchaseHeaderId = @HeaderId
            ORDER BY d.ItemSno";

        var details = (await _dbConnection.QueryAsync<ImportDetailRawDto>(detailSql, new { HeaderId = ih.Id })).ToList();
        return (ih, details);
    }

    private async Task<(ServiceHeaderRawDto?, List<ServiceLineRawDto>?)> FetchServiceDataAsync(int poId)
    {
        const string headerSql = @"
            SELECT sh.Id,
                   mSC.Description AS ServiceCategoryName,
                   mCT.Description AS ContractTypeName,
                   mFreq.Description AS FrequencyName,
                   sh.ValidityFrom, sh.ValidityTo,
                   sh.TotalOccurrences, sh.OverallLimit,
                   mMod.Description AS ModeOfDispatchName,
                   sh.FreightCharges, sh.TermDescription,
                   sh.DeliveryAddress, sh.BillingAddress
            FROM Purchase.PurchaseOrderServiceHeader sh WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster mSC ON mSC.Id = sh.ServiceCategoryId AND mSC.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mCT ON mCT.Id = sh.ContractTypeId AND mCT.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mFreq ON mFreq.Id = sh.FrequencyId AND mFreq.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mMod ON mMod.Id = sh.ModeOfDispatchId AND mMod.IsDeleted = 0
            WHERE sh.PurchaseOrderId = @PoId AND sh.IsDeleted = 0";

        var sh = await _dbConnection.QueryFirstOrDefaultAsync<ServiceHeaderRawDto>(headerSql, new { PoId = poId });
        if (sh == null) return (null, null);

        const string lineSql = @"
            SELECT sl.LineNo, sl.ServiceId, sl.ServiceCode, sl.ServiceDescription,
                   sl.UOMId, sl.PlannedQuantity, sl.PlannedRate, sl.PlannedValue,
                   sl.Discount, sl.GstPercent, sl.Remarks
            FROM Purchase.PurchaseOrderServiceLine sl WITH (NOLOCK)
            WHERE sl.ServicePoHeaderId = @HeaderId
            ORDER BY sl.LineNo";

        var lines = (await _dbConnection.QueryAsync<ServiceLineRawDto>(lineSql, new { HeaderId = sh.Id })).ToList();
        return (sh, lines);
    }

    // ──────────────────── DTO Assembly ────────────────────

    private static PurchaseOrderPrintDto AssembleDto(
        string poType,
        POHeaderRawDto header,
        LocalHeaderRawDto? localHeader,
        List<LocalDetailRawDto>? localDetails,
        ContractHeaderRawDto? contractHeader,
        List<ContractDetailRawDto>? contractDetails,
        ImportHeaderRawDto? importHeader,
        List<ImportDetailRawDto>? importDetails,
        ServiceHeaderRawDto? serviceHeader,
        List<ServiceLineRawDto>? serviceLines,
        List<PaymentTermRawDto> paymentTerms,
        CompanyDetailLookupDto? company,
        UnitDetailLookupDto? unit,
        PartyDetailLookupDto? vendor,
        CurrencyLookupDto? currency,
        PartyBankLookupDto? bank,
        Dictionary<int, ItemLookupDto> items,
        Dictionary<int, UOMLookupDto> uoms,
        Dictionary<int, DepartmentLookupDto> depts,
        Dictionary<int, string> states,
        Dictionary<int, string> cities,
        string? originCountryName)
    {
        // ── Company section (buyer / unit) ──
        var unitAddrParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(unit?.AddressLine1)) unitAddrParts.Add(unit.AddressLine1);
        if (!string.IsNullOrWhiteSpace(unit?.AddressLine2)) unitAddrParts.Add(unit.AddressLine2);

        var unitCityName = unit != null && unit.CityId > 0 && cities.TryGetValue(unit.CityId, out var uc) ? uc : null;
        var unitCityPin = unitCityName;
        if (unit != null && unit.PinCode > 0)
            unitCityPin = $"{unitCityName} - {unit.PinCode}";

        var companyDto = new POPrintCompanyDto
        {
            Name = company != null
                ? $"{company.LegalName ?? company.CompanyName} {unit?.UnitName}".Trim()
                : unit?.UnitName,
            Address = string.Join(", ", unitAddrParts),
            City = unitCityPin,
            Gstin = company?.GstNumber,
            Pan = company?.PanNumber,
            Cin = unit?.CINNO,
            Email = company?.Email,
            Web = company?.Website,
            Phone = unit?.Phone
        };

        // ── Registered Office ──
        var regAddrParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(company?.AddressLine1)) regAddrParts.Add(company.AddressLine1);
        if (!string.IsNullOrWhiteSpace(company?.AddressLine2)) regAddrParts.Add(company.AddressLine2);

        var companyCityName = company != null && company.CityId > 0 && cities.TryGetValue(company.CityId, out var cc) ? cc : null;
        var companyStateName = company != null && company.StateId > 0 && states.TryGetValue(company.StateId, out var cs) ? cs : null;

        var registeredOffice = company != null ? new POPrintRegisteredOfficeDto
        {
            Name = company.LegalName ?? company.CompanyName,
            Address = string.Join(", ", regAddrParts),
            City = companyCityName != null
                ? $"{companyCityName}{(companyStateName != null ? " " + companyStateName : "")}-{company.PinCode}"
                : null,
            Phone = company.Phone
        } : null;

        // ── PO Header ──
        string? incoterms = localHeader?.IncotermsName
                            ?? contractHeader?.IncotermsName
                            ?? importHeader?.IncotermsName;

        string? modeOfDispatch = localHeader?.ModeOfDispatchName
                                 ?? contractHeader?.ModeOfDispatchName
                                 ?? importHeader?.ModeOfDispatchName
                                 ?? serviceHeader?.ModeOfDispatchName;

        decimal? freightCharges = localHeader?.FreightCharges
                                  ?? contractHeader?.FreightCharges
                                  ?? importHeader?.FreightCharges
                                  ?? serviceHeader?.FreightCharges;

        string? termDescription = localHeader?.TermDescription
                                  ?? contractHeader?.TermDescription
                                  ?? importHeader?.TermDescription
                                  ?? serviceHeader?.TermDescription;

        var poHeaderDto = new POPrintHeaderDto
        {
            PONumber = header.PONumber,
            PODate = header.PODate.ToString("dd/MM/yyyy"),
            POCategory = header.POCategoryDescription,
            POMethod = header.POMethodDescription,
            CurrencyCode = currency?.Code,
            CurrencyName = currency?.Name,
            Status = header.StatusDescription,
            RevisionNo = header.RevisionNo,
            AmendmentReason = header.AmendmentReason,
            Incoterms = incoterms,
            ModeOfDispatch = modeOfDispatch,
            FreightCharges = freightCharges,
            TermDescription = termDescription
        };

        // ── Vendor section ──
        POPrintVendorDto? vendorDto = null;
        if (vendor != null)
        {
            var vendorCityName = vendor.CityId > 0 && cities.TryGetValue(vendor.CityId, out var vcity) ? vcity : null;
            var vendorStateName = vendor.StateId > 0 && states.TryGetValue(vendor.StateId, out var vstate) ? vstate : null;

            vendorDto = new POPrintVendorDto
            {
                Name = vendor.PartyName,
                Code = vendor.PartyCode,
                Address = vendor.AddressLine1,
                City = vendorCityName != null
                    ? $"{vendorCityName}{(!string.IsNullOrWhiteSpace(vendor.PostalCode) ? " - " + vendor.PostalCode : "")}"
                    : null,
                State = vendorStateName,
                StateCode = vendor.GSTStateCode?.ToString(),
                Gstin = vendor.GSTNumber,
                Pan = vendor.PAN,
                Phone = vendor.MobileNo ?? vendor.Phone,
                Email = vendor.EmailID
            };
        }

        // ── Delivery section ──
        string? deliveryAddress = localHeader?.DeliveryAddress
                                  ?? contractHeader?.DeliveryAddress
                                  ?? importHeader?.DeliveryAddress
                                  ?? serviceHeader?.DeliveryAddress;

        string? billingAddress = localHeader?.BillingAddress
                                 ?? contractHeader?.BillingAddress
                                 ?? importHeader?.BillingAddress
                                 ?? serviceHeader?.BillingAddress;

        bool isPartialReceipt = localHeader?.IsPartialReceiptAllowed
                                ?? contractHeader?.IsPartialReceiptAllowed
                                ?? importHeader?.IsPartialReceiptAllowed
                                ?? false;

        var deliveryDto = new POPrintDeliveryDto
        {
            DeliveryAddress = deliveryAddress,
            BillingAddress = billingAddress,
            IsPartialReceiptAllowed = isPartialReceipt
        };

        // ── Line Items (Local / Contract / Import) ──
        List<POPrintItemDto>? printItems = null;

        if (localDetails != null && localDetails.Count > 0)
        {
            printItems = localDetails.Select(d => MapItemDto(d.ItemSno, d.ItemId, d.UOMId, d.Quantity,
                d.UnitPrice, d.ItemValue, d.DiscountValue, d.GSTPercentage, d.CGSTPercentage,
                d.SGSTPercentage, d.IGSTPercentage, d.CGST, d.SGST, d.IGST,
                d.ScheduleDate, d.DepartmentId, null, null, null, null, null,
                items, uoms, depts)).ToList();
        }
        else if (contractDetails != null && contractDetails.Count > 0)
        {
            printItems = contractDetails.Select(d => MapItemDto(d.ItemSno, d.ItemId, d.UOMId, d.Quantity,
                d.UnitPrice, d.ItemValue, d.DiscountValue, d.GSTPercentage, d.CGSTPercentage,
                d.SGSTPercentage, d.IGSTPercentage, d.CGST, d.SGST, d.IGST,
                d.ScheduleDate, d.DepartmentId, null, null, null, null, null,
                items, uoms, depts)).ToList();
        }
        else if (importDetails != null && importDetails.Count > 0)
        {
            printItems = importDetails.Select(d => MapItemDto(d.ItemSno, d.ItemId, d.UomId, d.Quantity,
                d.UnitPrice, d.Quantity * d.UnitPrice, null, null, null, null, d.IGSTPercentage,
                null, null, d.IGST, null, null,
                d.FreightAmount, d.InsuranceAmount, d.CIFValue, d.BasicCustomDuty, d.TotalValue,
                items, uoms, depts)).ToList();
        }

        // ── Service Lines ──
        List<POPrintServiceLineDto>? printServiceLines = null;
        if (serviceLines != null && serviceLines.Count > 0)
        {
            printServiceLines = serviceLines.Select(sl =>
            {
                var uomName = sl.UOMId.HasValue && uoms.TryGetValue(sl.UOMId.Value, out var u) ? u.UOMName : null;
                return new POPrintServiceLineDto
                {
                    LineNo = sl.LineNo,
                    ServiceCode = sl.ServiceCode,
                    ServiceDescription = sl.ServiceDescription,
                    UOMName = uomName,
                    PlannedQuantity = sl.PlannedQuantity,
                    PlannedRate = sl.PlannedRate,
                    PlannedValue = sl.PlannedValue,
                    Discount = sl.Discount,
                    GstPercent = sl.GstPercent,
                    Remarks = sl.Remarks
                };
            }).ToList();
        }

        // ── Totals ──
        var totalsDto = new POPrintTotalsDto
        {
            ItemTotal = header.ItemTotal,
            DiscountTotal = header.DiscountTotal,
            PandFTotal = header.PandFTotal,
            MiscCharges = header.MiscCharges,
            GSTTotal = header.GSTTotal,
            CGSTTotal = header.CGSTTotal,
            SGSTTotal = header.SGSTTotal,
            IGSTTotal = header.IGSTTotal,
            FreightTotal = header.FreightTotal,
            InsuranceTotal = header.InsuranceTotal,
            TDSTotal = header.TDSTotal,
            AdvanceAmount = header.AdvanceAmount,
            PurchaseValue = header.PurchaseValue,
            PurchaseValueInWords = ConvertAmountToWords(header.PurchaseValue)
        };

        // ── Payment Terms ──
        var printPaymentTerms = paymentTerms.Select(pt => new POPrintPaymentTermDto
        {
            PaymentTermName = pt.PaymentTermName,
            AdvancePercent = pt.AdvancePercent,
            CreditDays = pt.CreditDays,
            PaymentModeName = pt.PaymentModeName,
            InsuranceAmount = pt.InsuranceAmount,
            AdvanceAmount = pt.AdvanceAmount,
            BalanceAmount = pt.BalanceAmount
        }).ToList();

        // ── Bank ──
        POPrintBankDto? bankDto = null;
        if (bank != null)
        {
            bankDto = new POPrintBankDto
            {
                Name = bank.BankName,
                Branch = bank.BankBranch,
                AccountNo = bank.BankAccountNumber,
                Ifsc = bank.IFSCCode
            };
        }

        // ── Type-specific sections ──
        POPrintImportDto? importDto = null;
        if (poType == "Import" && importHeader != null)
        {
            importDto = new POPrintImportDto
            {
                TTExchangeRate = importHeader.TTExchangeRate,
                ShippingPortName = importHeader.ShippingPortName,
                DestinationPortName = importHeader.DestinationPortName,
                ModeOfTransportName = importHeader.ModeOfTransportName,
                ShippingModeName = importHeader.ShippingModeName,
                OriginCountryName = originCountryName,
                BillOfLadingNumber = importHeader.BillOfLadingNumber,
                VesselName = importHeader.VesselName,
                ContainerNumber = importHeader.ContainerNumber,
                ExpectedDeparture = importHeader.ExpectedTimeOfDeparture.ToString("dd/MM/yyyy"),
                ExpectedArrival = importHeader.ExpectedTimeOfArrival.ToString("dd/MM/yyyy"),
                LCNumber = importHeader.LCNumber,
                LCDate = importHeader.LCDate?.ToString("dd/MM/yyyy"),
                LCAmount = importHeader.LCAmount,
                TTReferenceNumber = importHeader.TTReferenceNumber,
                TTTransferDate = importHeader.TTTransferDate?.ToString("dd/MM/yyyy")
            };
        }

        POPrintContractDto? contractDto = null;
        if (poType == "Contract" && contractHeader != null)
        {
            contractDto = new POPrintContractDto
            {
                ContractPONumber = contractHeader.ContractPONumber,
                ContractPOHeaderId = contractHeader.ContractPOHeaderId
            };
        }

        POPrintServiceHeaderDto? serviceDto = null;
        if (poType == "Service" && serviceHeader != null)
        {
            serviceDto = new POPrintServiceHeaderDto
            {
                ServiceCategoryName = serviceHeader.ServiceCategoryName,
                ContractTypeName = serviceHeader.ContractTypeName,
                FrequencyName = serviceHeader.FrequencyName,
                ValidityFrom = serviceHeader.ValidityFrom?.ToString("dd/MM/yyyy"),
                ValidityTo = serviceHeader.ValidityTo?.ToString("dd/MM/yyyy"),
                TotalOccurrences = serviceHeader.TotalOccurrences,
                OverallLimit = serviceHeader.OverallLimit
            };
        }

        return new PurchaseOrderPrintDto
        {
            POType = poType,
            Company = companyDto,
            RegisteredOffice = registeredOffice,
            PurchaseOrder = poHeaderDto,
            Vendor = vendorDto,
            Delivery = deliveryDto,
            Items = printItems,
            ServiceLines = printServiceLines,
            Totals = totalsDto,
            PaymentTerms = printPaymentTerms,
            Bank = bankDto,
            ImportDetails = importDto,
            ContractDetails = contractDto,
            ServiceDetails = serviceDto
        };
    }

    // ──────────────────── Helpers ────────────────────

    private static POPrintItemDto MapItemDto(
        int sno, int itemId, int uomId, decimal qty, decimal unitPrice, decimal itemValue,
        decimal? discountValue, decimal? gstPct, decimal? cgstPct, decimal? sgstPct, decimal? igstPct,
        decimal? cgst, decimal? sgst, decimal? igst,
        DateTimeOffset? scheduleDate, int? deptId,
        decimal? freightAmount, decimal? insuranceAmount, decimal? cifValue, decimal? basicCustomDuty, decimal? totalValue,
        Dictionary<int, ItemLookupDto> items,
        Dictionary<int, UOMLookupDto> uoms,
        Dictionary<int, DepartmentLookupDto> depts)
    {
        var item = items.TryGetValue(itemId, out var i) ? i : null;
        var uom = uoms.TryGetValue(uomId, out var u) ? u : null;
        var dept = deptId.HasValue && depts.TryGetValue(deptId.Value, out var d) ? d : null;

        return new POPrintItemDto
        {
            SNo = sno,
            ItemCode = item?.ItemCode,
            ItemName = item?.ItemName,
            HSNCode = item?.HSNCode,
            UOMName = uom?.UOMName,
            Quantity = qty,
            UnitPrice = unitPrice,
            ItemValue = itemValue,
            DiscountValue = discountValue,
            GSTPercentage = gstPct,
            CGSTPercentage = cgstPct,
            SGSTPercentage = sgstPct,
            IGSTPercentage = igstPct,
            CGST = cgst,
            SGST = sgst,
            IGST = igst,
            ScheduleDate = scheduleDate?.ToString("dd/MM/yyyy"),
            DepartmentName = dept?.DepartmentName,
            FreightAmount = freightAmount,
            InsuranceAmount = insuranceAmount,
            CIFValue = cifValue,
            BasicCustomDuty = basicCustomDuty,
            TotalValue = totalValue
        };
    }

    private static string ConvertAmountToWords(decimal amount)
    {
        var intPart = (long)Math.Floor(amount);
        if (intPart == 0) return "Rs. Zero only";
        return $"Rs. {ConvertNumberToWords(intPart)} only";
    }

    private static string ConvertNumberToWords(long number)
    {
        if (number == 0) return "Zero";

        string[] ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
            "Seventeen", "Eighteen", "Nineteen" };

        string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        var words = string.Empty;

        if (number / 10000000 > 0)
        {
            words += ConvertNumberToWords(number / 10000000) + " Crore ";
            number %= 10000000;
        }
        if (number / 100000 > 0)
        {
            words += ConvertNumberToWords(number / 100000) + " Lakh ";
            number %= 100000;
        }
        if (number / 1000 > 0)
        {
            words += ConvertNumberToWords(number / 1000) + " Thousand ";
            number %= 1000;
        }
        if (number / 100 > 0)
        {
            words += ConvertNumberToWords(number / 100) + " Hundred ";
            number %= 100;
        }
        if (number > 0)
        {
            if (number < 20)
                words += ones[number];
            else
            {
                words += tens[number / 10];
                if (number % 10 > 0)
                    words += " " + ones[number % 10];
            }
        }

        return words.Trim();
    }
}
