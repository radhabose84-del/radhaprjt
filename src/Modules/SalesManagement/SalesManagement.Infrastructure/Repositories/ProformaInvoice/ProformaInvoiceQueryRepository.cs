using System.Data;
using System.Text;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;

namespace SalesManagement.Infrastructure.Repositories.ProformaInvoice
{
    public class ProformaInvoiceQueryRepository : IProformaInvoiceQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IUnitDetailLookup _unitDetailLookup;
        private readonly ICompanyDetailLookup _companyDetailLookup;
        private readonly IPartyDetailLookup _partyDetailLookup;
        private readonly IPartyBankLookup _partyBankLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICityLookup _cityLookup;

        public ProformaInvoiceQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IUnitDetailLookup unitDetailLookup,
            ICompanyDetailLookup companyDetailLookup,
            IPartyDetailLookup partyDetailLookup,
            IPartyBankLookup partyBankLookup,
            IItemLookup itemLookup,
            IStateLookup stateLookup,
            ICityLookup cityLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _unitDetailLookup = unitDetailLookup;
            _companyDetailLookup = companyDetailLookup;
            _partyDetailLookup = partyDetailLookup;
            _partyBankLookup = partyBankLookup;
            _itemLookup = itemLookup;
            _stateLookup = stateLookup;
            _cityLookup = cityLookup;
        }

        public async Task<(List<ProformaInvoiceDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (p.ProformaNumber LIKE @Search OR so.SalesOrderNo LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.ProformaInvoice p
                LEFT JOIN Sales.SalesOrderHeader so ON p.SalesOrderId = so.Id AND so.IsDeleted = 0
                WHERE p.IsDeleted = 0 {searchFilter};

                SELECT p.Id, p.ProformaNumber, p.ProformaDate,
                    p.SalesOrderId,
                    so.SalesOrderNo,
                    p.PartyId,
                    p.ProformaAmount, p.SOBalance, p.PaymentReceivedAmount,
                    CAST(CASE WHEN p.PaymentReceivedAmount > 0 THEN 1 ELSE 0 END AS BIT) AS PaymentReceivedFlag,
                    p.StatusId,
                    mm.Description AS StatusName,
                    p.Remarks,
                    p.IsActive, p.IsDeleted,
                    p.CreatedBy, p.CreatedDate, p.CreatedByName,
                    p.ModifiedBy, p.ModifiedDate, p.ModifiedByName
                FROM Sales.ProformaInvoice p
                LEFT JOIN Sales.SalesOrderHeader so ON p.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON p.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE p.IsDeleted = 0 {searchFilter}
                ORDER BY p.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<ProformaInvoiceDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in list)
                {
                    item.CustomerName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<ProformaInvoiceDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT p.Id, p.ProformaNumber, p.ProformaDate,
                    p.SalesOrderId,
                    so.SalesOrderNo,
                    p.PartyId,
                    p.ProformaAmount, p.SOBalance, p.PaymentReceivedAmount,
                    CAST(CASE WHEN p.PaymentReceivedAmount > 0 THEN 1 ELSE 0 END AS BIT) AS PaymentReceivedFlag,
                    p.StatusId,
                    mm.Description AS StatusName,
                    p.Remarks,
                    p.IsActive, p.IsDeleted,
                    p.CreatedBy, p.CreatedDate, p.CreatedByName,
                    p.ModifiedBy, p.ModifiedDate, p.ModifiedByName
                FROM Sales.ProformaInvoice p
                LEFT JOIN Sales.SalesOrderHeader so ON p.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON p.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE p.Id = @Id AND p.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<ProformaInvoiceDto>(sql, new { Id = id });

            if (dto != null)
            {
                var party = await _partyLookup.GetByIdAsync(dto.PartyId);
                dto.CustomerName = party?.PartyName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<ProformaInvoiceLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, ProformaNumber, ProformaDate, ProformaAmount
                FROM Sales.ProformaInvoice
                WHERE IsActive = 1 AND IsDeleted = 0
                AND ProformaNumber LIKE @Term
                ORDER BY ProformaNumber ASC";

            var result = await _dbConnection.QueryAsync<ProformaInvoiceLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<List<ProformaInvoiceDto>> GetBySalesOrderIdAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT p.Id, p.ProformaNumber, p.ProformaDate,
                    p.SalesOrderId,
                    so.SalesOrderNo,
                    p.PartyId,
                    p.ProformaAmount, p.SOBalance, p.PaymentReceivedAmount,
                    CAST(CASE WHEN p.PaymentReceivedAmount > 0 THEN 1 ELSE 0 END AS BIT) AS PaymentReceivedFlag,
                    p.StatusId,
                    mm.Description AS StatusName,
                    p.Remarks,
                    p.IsActive, p.IsDeleted,
                    p.CreatedBy, p.CreatedDate, p.CreatedByName,
                    p.ModifiedBy, p.ModifiedDate, p.ModifiedByName
                FROM Sales.ProformaInvoice p
                LEFT JOIN Sales.SalesOrderHeader so ON p.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON p.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE p.SalesOrderId = @SalesOrderId AND p.IsDeleted = 0
                ORDER BY p.Id DESC";

            var list = (await _dbConnection.QueryAsync<ProformaInvoiceDto>(sql, new { SalesOrderId = salesOrderId })).ToList();

            if (list.Count > 0)
            {
                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in list)
                {
                    item.CustomerName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;
                }
            }

            return list;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.ProformaInvoice
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SalesOrderExistsAndApprovedAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesOrderHeader soh
                    INNER JOIN Sales.MiscMaster mm ON soh.StatusId = mm.Id AND mm.IsDeleted = 0
                    INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                    WHERE soh.Id = @Id AND soh.IsDeleted = 0
                      AND LOWER(mtm.MiscTypeCode) = LOWER('ApprovalStatus')
                      AND LOWER(mm.Code) = LOWER('Approved')
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderId });
        }

        public async Task<bool> SalesOrderHasAdvancePaymentTypeAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesOrderHeader soh
                    INNER JOIN Sales.MiscMaster mm ON soh.PaymentTypeId = mm.Id AND mm.IsDeleted = 0
                    INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                    WHERE soh.Id = @Id AND soh.IsDeleted = 0
                      AND LOWER(mtm.MiscTypeCode) = LOWER('PaymentType')
                      AND LOWER(mm.Code) = LOWER('Advance')
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderId });
        }

        public async Task<decimal> GetSalesOrderBalanceAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT ISNULL(soh.FinalAmount, 0) - ISNULL(
                    (SELECT SUM(p.ProformaAmount)
                     FROM Sales.ProformaInvoice p
                     WHERE p.SalesOrderId = @Id AND p.IsDeleted = 0), 0)
                FROM Sales.SalesOrderHeader soh
                WHERE soh.Id = @Id AND soh.IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<decimal>(sql, new { Id = salesOrderId });
        }

        public async Task<bool> IsDraftStatusAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.ProformaInvoice p
                    INNER JOIN Sales.MiscMaster mm ON p.StatusId = mm.Id AND mm.IsDeleted = 0
                    INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                    WHERE p.Id = @Id AND p.IsDeleted = 0
                      AND LOWER(mtm.MiscTypeCode) = LOWER('ProformaInvStatus')
                      AND LOWER(mm.Code) = LOWER('Draft')
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> StatusExistsAsync(int statusId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.MiscMaster mm
                    INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                    WHERE mm.Id = @Id AND mm.IsDeleted = 0
                      AND LOWER(mtm.MiscTypeCode) = LOWER('ProformaInvStatus')
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = statusId });
        }

        public async Task<decimal> GetProformaAmountAsync(int id)
        {
            const string sql = @"
                SELECT ISNULL(ProformaAmount, 0)
                FROM Sales.ProformaInvoice
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<decimal>(sql, new { Id = id });
        }

        public async Task<bool> HasReceivedAdvancePaymentAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.ProformaInvoice p
                    WHERE p.SalesOrderId = @Id AND p.IsDeleted = 0
                      AND p.PaymentReceivedAmount > 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderId });
        }

        public async Task<ProformaInvoicePrintDto?> GetPrintDetailsAsync(int id)
        {
            // Step 1: Proforma Invoice header joined with SalesOrderHeader
            const string headerSql = @"
                SELECT
                    pi.Id, pi.ProformaNumber, pi.ProformaDate,
                    pi.SalesOrderId, pi.PartyId,
                    soh.SalesOrderNo, soh.OrderDate AS SalesOrderDate,
                    soh.UnitId, soh.AgentId,
                    soh.TaxableAmount, soh.TotalFreight,
                    soh.GSTPercentage, soh.TotalGST,
                    soh.TCSPercentage, soh.TotalTCS,
                    soh.FinalAmount,
                    pi.Remarks
                FROM Sales.ProformaInvoice pi
                INNER JOIN Sales.SalesOrderHeader soh
                    ON pi.SalesOrderId = soh.Id AND soh.IsDeleted = 0
                WHERE pi.Id = @Id AND pi.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<ProformaPrintHeaderRawDto>(
                headerSql, new { Id = id });

            if (header == null)
                return null;

            // Step 2: Sales Order line items
            const string itemsSql = @"
                SELECT
                    ROW_NUMBER() OVER (ORDER BY sod.Id) AS SNo,
                    sod.ItemId, sod.HSNId,
                    sod.QtyInBags   AS NoOfPacks,
                    sod.TotalWeight AS QuantityKg,
                    (sod.ExMillRate - sod.DiscountPerUnit) AS Rate,
                    sod.TaxableAmount AS Amount,
                    sod.TaxPercentage
                FROM Sales.SalesOrderDetail sod
                WHERE sod.SalesOrderHeaderId = @SalesOrderId
                ORDER BY sod.Id ASC";

            var rawItems = (await _dbConnection.QueryAsync<ProformaPrintItemRawDto>(
                itemsSql, new { SalesOrderId = header.SalesOrderId })).ToList();

            // Step 3: Cross-module lookups (run in parallel)
            var unitDetailTask    = _unitDetailLookup.GetByIdAsync(header.UnitId);
            var companyDetailTask = _companyDetailLookup.GetByUnitIdAsync(header.UnitId);
            var partyDetailTask   = _partyDetailLookup.GetByIdAsync(header.PartyId);

            await Task.WhenAll(unitDetailTask, companyDetailTask, partyDetailTask);

            var unitDetail    = await unitDetailTask;
            var companyDetail = await companyDetailTask;
            var partyDetail   = await partyDetailTask;

            // Step 4: Item name lookup
            var itemIds  = rawItems.Select(x => x.ItemId).Distinct();
            var itemList = await _itemLookup.GetByIdsAsync(itemIds);
            var itemDict = itemList.ToDictionary(i => i.Id, i => i);

            // Step 5: Agent lookup (optional)
            Contracts.Dtos.Lookups.Party.PartyDetailLookupDto? agentDetail = null;
            if (header.AgentId.HasValue)
                agentDetail = await _partyDetailLookup.GetByIdAsync(header.AgentId.Value);

            // Step 6: State / City lookups (batch)
            var stateIds = new HashSet<int>();
            var cityIds  = new HashSet<int>();

            if (unitDetail    != null) { stateIds.Add(unitDetail.StateId);    cityIds.Add(unitDetail.CityId); }
            if (companyDetail != null) { stateIds.Add(companyDetail.StateId); cityIds.Add(companyDetail.CityId); }
            if (partyDetail   != null) { stateIds.Add(partyDetail.StateId);   cityIds.Add(partyDetail.CityId); }

            var statesTask = _stateLookup.GetByIdsAsync(stateIds);
            var citiesTask = _cityLookup.GetByIdsAsync(cityIds);
            await Task.WhenAll(statesTask, citiesTask);

            var stateDict = (await statesTask).ToDictionary(s => s.StateId,  s => s.StateName);
            var cityDict  = (await citiesTask).ToDictionary(c => c.CityId,   c => c.CityName);

            // Step 7: Bank details (uses company GSTIN)
            ProformaInvoicePrintBankDto? bank = null;
            if (!string.IsNullOrWhiteSpace(companyDetail?.GstNumber))
            {
                var bankDto = await _partyBankLookup.GetDefaultBankByGstAsync(companyDetail.GstNumber);
                if (bankDto != null)
                {
                    bank = new ProformaInvoicePrintBankDto
                    {
                        BankName  = bankDto.BankName,
                        AccountNo = bankDto.BankAccountNumber,
                        IfscCode  = bankDto.IFSCCode,
                        Branch    = bankDto.BankBranch
                    };
                }
            }

            // Step 8: Determine CGST/SGST vs IGST (compare unit state with buyer state)
            bool isIntraState = unitDetail?.StateId == partyDetail?.StateId;
            decimal cgstRate   = isIntraState ? header.GSTPercentage / 2 : 0m;
            decimal sgstRate   = isIntraState ? header.GSTPercentage / 2 : 0m;
            decimal igstRate   = isIntraState ? 0m : header.GSTPercentage;
            decimal cgstAmount = isIntraState ? header.TotalGST / 2 : 0m;
            decimal sgstAmount = isIntraState ? header.TotalGST / 2 : 0m;
            decimal igstAmount = isIntraState ? 0m : header.TotalGST;

            // Step 9: Round-off = FinalAmount − (ValueOfSupply + GST + TCS)
            decimal preRound = header.TaxableAmount + header.TotalGST + header.TotalTCS;
            decimal roundOff = header.FinalAmount - preRound;

            // Build line items
            var printItems = rawItems.Select(raw =>
            {
                itemDict.TryGetValue(raw.ItemId, out var item);
                return new ProformaInvoicePrintItemDto
                {
                    SNo             = raw.SNo,
                    ProductCategory = item?.ParentItemName,
                    ProductName     = item?.ItemName,
                    HsnGroup        = item?.TariffNumber,
                    HsnCode         = item?.HSNCode,
                    NoPacks         = raw.NoOfPacks,
                    QuantityKg      = raw.QuantityKg,
                    Rate            = raw.Rate,
                    Amount          = raw.Amount
                };
            }).ToList();

            // Build company (unit) section
            var company = new ProformaInvoicePrintCompanyDto
            {
                Name    = unitDetail?.UnitName,
                Address = JoinAddressLines(unitDetail?.AddressLine1, unitDetail?.AddressLine2),
                City    = unitDetail != null && cityDict.TryGetValue(unitDetail.CityId, out var unitCity)
                            ? unitCity : null,
                Gstin   = companyDetail?.GstNumber,
                Pan     = companyDetail?.PanNumber,
                Cin     = unitDetail?.CINNO,
                Email   = companyDetail?.Email,
                Web     = companyDetail?.Website,
                Phone   = unitDetail?.Phone
            };

            // Build registered office section
            ProformaInvoicePrintRegisteredOfficeDto? registeredOffice = null;
            if (companyDetail != null)
            {
                registeredOffice = new ProformaInvoicePrintRegisteredOfficeDto
                {
                    Address = JoinAddressLines(companyDetail.AddressLine1, companyDetail.AddressLine2),
                    City    = cityDict.TryGetValue(companyDetail.CityId, out var regCity) ? regCity : null,
                    Phone   = companyDetail.Phone,
                    Email   = companyDetail.Email
                };
            }

            // Build billed-to party section
            ProformaInvoicePrintPartyDto? billedTo = null;
            if (partyDetail != null)
            {
                billedTo = new ProformaInvoicePrintPartyDto
                {
                    Name      = partyDetail.PartyName,
                    Address   = JoinAddressLines(partyDetail.AddressLine1, partyDetail.AddressLine2),
                    City      = cityDict.TryGetValue(partyDetail.CityId, out var partyCity) ? partyCity : null,
                    State     = stateDict.TryGetValue(partyDetail.StateId, out var partySt) ? partySt : null,
                    StateCode = partyDetail.GSTStateCode?.ToString(),
                    Gstin     = partyDetail.GSTNumber,
                    Pan       = partyDetail.PAN,
                    Phone     = partyDetail.MobileNo ?? partyDetail.Phone
                };
            }

            // Build agent section
            ProformaInvoicePrintAgentDto? agent = agentDetail != null
                ? new ProformaInvoicePrintAgentDto
                  {
                      Name  = agentDetail.PartyName,
                      Gstin = agentDetail.GSTNumber
                  }
                : null;

            return new ProformaInvoicePrintDto
            {
                Company         = company,
                RegisteredOffice = registeredOffice,
                Header = new ProformaInvoicePrintHeaderDto
                {
                    PiNumber      = header.ProformaNumber,
                    PiDate        = header.ProformaDate.ToString("dd/MM/yy"),
                    SalesOrderNo  = header.SalesOrderNo,
                    SalesOrderDate = header.SalesOrderDate.ToString("dd/MM/yy")
                },
                BilledTo = billedTo,
                Agent    = agent,
                Items    = printItems,
                Totals   = new ProformaInvoicePrintTotalsDto
                {
                    TotalPacks         = rawItems.Sum(x => x.NoOfPacks),
                    TotalQtyKg         = rawItems.Sum(x => x.QuantityKg),
                    TotalItemValue     = rawItems.Sum(x => x.Amount),
                    Discount           = 0m,
                    Freight            = header.TotalFreight,
                    Insurance          = 0m,
                    HandlingCharges    = 0m,
                    OtherCharges       = 0m,
                    ValueOfSupply      = header.TaxableAmount,
                    CgstRate           = cgstRate,
                    CgstAmount         = cgstAmount,
                    SgstRate           = sgstRate,
                    SgstAmount         = sgstAmount,
                    IgstRate           = igstRate,
                    IgstAmount         = igstAmount,
                    TcsRate            = header.TCSPercentage,
                    TcsAmount          = header.TotalTCS,
                    RoundOff           = roundOff,
                    InvoiceAmount      = header.FinalAmount,
                    InvoiceAmountWords = ConvertAmountToWords(header.FinalAmount),
                    Remarks            = header.Remarks
                },
                Bank = bank
            };
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static string? JoinAddressLines(string? line1, string? line2)
        {
            var parts = new[] { line1, line2 }.Where(s => !string.IsNullOrWhiteSpace(s));
            var joined = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(joined) ? null : joined;
        }

        private static string ConvertAmountToWords(decimal amount)
        {
            long rupees  = (long)Math.Floor(amount);
            int  paise   = (int)Math.Round((amount - rupees) * 100);

            var sb = new StringBuilder("RS. ");
            sb.Append(ConvertNumberToWords(rupees).ToUpper());

            if (paise > 0)
            {
                sb.Append(" AND ");
                sb.Append(ConvertNumberToWords(paise).ToUpper());
                sb.Append(" PAISE");
            }

            sb.Append(" ONLY");
            return sb.ToString();
        }

        private static string ConvertNumberToWords(long number)
        {
            if (number == 0) return "ZERO";
            if (number < 0)  return "MINUS " + ConvertNumberToWords(-number);

            string[] ones  = { "", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE",
                                "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN",
                                "SEVENTEEN", "EIGHTEEN", "NINETEEN" };
            string[] tens  = { "", "", "TWENTY", "THIRTY", "FORTY", "FIFTY",
                                "SIXTY", "SEVENTY", "EIGHTY", "NINETY" };

            if (number < 20)        return ones[number];
            if (number < 100)       return tens[number / 10] + (number % 10 > 0 ? " " + ones[number % 10] : "");
            if (number < 1_000)     return ones[number / 100] + " HUNDRED" + (number % 100 > 0 ? " " + ConvertNumberToWords(number % 100) : "");
            if (number < 1_00_000)  return ConvertNumberToWords(number / 1_000) + " THOUSAND" + (number % 1_000 > 0 ? " " + ConvertNumberToWords(number % 1_000) : "");
            if (number < 1_00_00_000) return ConvertNumberToWords(number / 1_00_000) + " LAKH" + (number % 1_00_000 > 0 ? " " + ConvertNumberToWords(number % 1_00_000) : "");

            return ConvertNumberToWords(number / 1_00_00_000) + " CRORE" + (number % 1_00_00_000 > 0 ? " " + ConvertNumberToWords(number % 1_00_00_000) : "");
        }
    }
}
