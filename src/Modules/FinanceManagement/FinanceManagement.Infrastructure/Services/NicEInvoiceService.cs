using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using Microsoft.Extensions.Configuration;

namespace FinanceManagement.Infrastructure.Services
{
    /// <summary>
    /// Implements the NIC e-invoice API flow:
    /// 1. Authenticate → get AuthToken + encrypted Sek
    /// 2. Decrypt Sek using AppKey (AES-256 ECB)
    /// 3. Load EInvoiceHeader + EInvoiceDetail from Finance schema
    /// 4. Load seller info (Company GSTIN/address) via AppData schema queries
    /// 5. Load buyer info (Party name via lookup, address via PartyManagement schema query)
    /// 6. Build NIC v1.04 JSON payload
    /// 7. AES-256 ECB encrypt payload with Sek → Base64
    /// 8. POST to NIC Generate IRN endpoint
    /// 9. AES-256 ECB decrypt response data with Sek
    /// 10. Return IRN, AckNo, AckDate, SignedInvoice, SignedQRCode
    ///
    /// Configuration section: "NicEInvoice" in appsettings.json
    /// </summary>
    public class NicEInvoiceService : INicEInvoiceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IConfiguration _configuration;

        // ── JSON serialiser options (camelCase disabled; NIC uses PascalCase) ──
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public NicEInvoiceService(
            IHttpClientFactory httpClientFactory,
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IPartyLookup partyLookup,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _partyLookup = partyLookup;
            _configuration = configuration;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Public entry point
        // ─────────────────────────────────────────────────────────────────────

        public async Task<NicIrnResultDto> GenerateIrnAsync(
            int eInvoiceHeaderId,
            EwbTransportDetails? ewbDetails = null,
            CancellationToken ct = default)
        {
            try
            {
                // 1. Load all data needed for the NIC JSON payload
                var data = await LoadNicDataAsync(eInvoiceHeaderId, ct);

                // 2. Pre-flight NIC validations (catch errors before calling API)
                var preflightErrors = ValidateNicPayloadData(data);
                if (preflightErrors.Count > 0)
                {
                    return new NicIrnResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "VALIDATION_ERROR",
                        ErrorMessage = string.Join(" | ", preflightErrors)
                    };
                }

                // 3. Authenticate with NIC
                var (authToken, sek) = await GetAuthTokenAsync(ct);

                // 4. Build NIC JSON (with optional EwbDtls) and encrypt with Sek
                var jsonPayload = BuildNicPayload(data, ewbDetails);
                var jsonString = JsonSerializer.Serialize(jsonPayload, _jsonOptions);
                var encryptedPayload = AesEncryptEcb(jsonString, sek);

                // 5. Call NIC Generate IRN API
                return await CallGenerateIrnAsync(encryptedPayload, authToken, sek, ct);
            }
            catch (Exception ex)
            {
                return new NicIrnResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "SERVICE_ERROR",
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<NicEwbResultDto> GenerateEwbAsync(
            int eInvoiceHeaderId,
            string transporterId,
            string transporterName,
            string transMode,
            int distance,
            string transDocNo,
            string transDocDt,
            string vehicleNo,
            string vehicleType,
            CancellationToken ct = default)
        {
            try
            {
                // 1. Authenticate with NIC (reuse same auth flow)
                var (authToken, sek) = await GetAuthTokenAsync(ct);

                // 2. Load IRN from EInvoiceHeader
                const string irnSql = @"
                    SELECT IrnNumber
                    FROM Finance.EInvoiceHeader
                    WHERE Id = @id AND IsDeleted = 0";

                var irnNumber = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                    irnSql, new { id = eInvoiceHeaderId });

                if (string.IsNullOrEmpty(irnNumber))
                    return new NicEwbResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "VALIDATION_ERROR",
                        ErrorMessage = $"EInvoiceHeader {eInvoiceHeaderId} has no IRN generated. Generate IRN first before creating e-Waybill."
                    };

                // NIC Error 4038: Distance validation (must be between 1 and 4000 km)
                if (distance > 4000)
                    return new NicEwbResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "VALIDATION_ERROR",
                        ErrorMessage = $"Distance {distance} km exceeds maximum allowed (4000 km). Verify the distance between source and destination pincodes."
                    };

                // Vehicle number format check
                if (!string.IsNullOrEmpty(vehicleNo))
                {
                    var cleanVehNo = vehicleNo.Replace(" ", "").Replace("-", "");
                    if (cleanVehNo.Length < 5 || cleanVehNo.Length > 15)
                        return new NicEwbResultDto
                        {
                            IsSuccess = false,
                            ErrorCode = "VALIDATION_ERROR",
                            ErrorMessage = $"Vehicle number '{vehicleNo}' has invalid length. Expected 5-15 characters (e.g., TN38AB1234)."
                        };
                }

                // 3. Build e-Waybill request JSON
                var ewbPayload = new
                {
                    Irn = irnNumber,
                    Distance = distance,
                    TransMode = transMode,
                    TransId = transporterId,
                    TransName = transporterName,
                    TrnDocDt = transDocDt,
                    TrnDocNo = transDocNo,
                    VehNo = vehicleNo,
                    VehType = vehicleType
                };

                var jsonString = JsonSerializer.Serialize(ewbPayload, _jsonOptions);
                var encryptedPayload = AesEncryptEcb(jsonString, sek);

                // 4. Call NIC e-Waybill API
                return await CallGenerateEwbAsync(encryptedPayload, authToken, sek, ct);
            }
            catch (Exception ex)
            {
                return new NicEwbResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "SERVICE_ERROR",
                    ErrorMessage = ex.Message
                };
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Pre-flight NIC validations (based on actual NIC error codes)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Validates data BEFORE calling NIC API. Catches common NIC rejection reasons locally
        /// so the user gets clear error messages instead of cryptic NIC error codes.
        /// Based on actual NIC errors encountered during testing:
        ///   5002 — DocNo invalid format/length
        ///   2311 — HSN code must be 6+ digits (AATO >= 5 Cr)
        ///   2227 — CGST and SGST amounts must be equal (intra-state)
        ///   2189 — TotInvVal does not match calculated formula
        ///   4038 — Distance between pincodes too high
        /// </summary>
        private static List<string> ValidateNicPayloadData(NicEInvoiceData data)
        {
            var errors = new List<string>();

            // ── Seller GSTIN must be present ────────────────────────────────
            if (string.IsNullOrWhiteSpace(data.CompanyGstin))
                errors.Add("Seller GSTIN is missing. Ensure the Unit's Company has a valid GST Number.");

            // ── Buyer GSTIN must be present ─────────────────────────────────
            if (string.IsNullOrWhiteSpace(data.GstNo))
                errors.Add("Buyer GSTIN is missing. Ensure the Party has a valid GST Number.");

            // ── NIC Error 5002: DocNo max 16 chars, pattern validation ──────
            var docNo = data.InvoiceNo ?? string.Empty;
            if (string.IsNullOrWhiteSpace(docNo))
                errors.Add("Invoice Number is empty.");
            else if (docNo.Length > 16)
                errors.Add($"Invoice Number '{docNo}' exceeds 16 characters (NIC limit). Current length: {docNo.Length}.");

            // ── NIC Error 2311: HSN code must be minimum 6 digits ───────────
            foreach (var detail in data.Details)
            {
                var hsn = detail.HsnNo ?? string.Empty;
                if (hsn.Length < 6)
                    errors.Add($"Item '{detail.ItemName}' (Line {detail.ItemSno}): HSN code '{hsn}' must be at least 6 digits. Current: {hsn.Length} digits.");
            }

            // ── NIC Error 2227: CGST and SGST must be equal (intra-state) ───
            var sellerState = data.CompanyGstin?.Length >= 2 ? data.CompanyGstin.Substring(0, 2) : string.Empty;
            var buyerState = data.GstNo?.Length >= 2 ? data.GstNo.Substring(0, 2) : string.Empty;
            var isIntraState = sellerState == buyerState && !string.IsNullOrEmpty(sellerState);

            if (isIntraState)
            {
                // Header-level CGST and SGST must be equal
                if (data.CGST != data.SGST)
                    errors.Add($"Intra-state supply: Header CGST ({data.CGST}) and SGST ({data.SGST}) must be equal.");

                // Item-level CGST and SGST must be equal
                foreach (var detail in data.Details)
                {
                    if (detail.CGST != detail.SGST)
                        errors.Add($"Item '{detail.ItemName}' (Line {detail.ItemSno}): CGST ({detail.CGST}) and SGST ({detail.SGST}) must be equal for intra-state supply.");
                }

                // Intra-state should not have IGST
                if (data.IGST != 0)
                    errors.Add($"Intra-state supply: IGST should be 0 but found {data.IGST}. Use CGST + SGST instead.");
            }
            else
            {
                // Inter-state should not have CGST/SGST
                if (data.CGST != 0 || data.SGST != 0)
                    errors.Add($"Inter-state supply: CGST ({data.CGST}) and SGST ({data.SGST}) should be 0. Use IGST instead.");
            }

            // ── NIC Error 2189: TotInvVal formula validation ────────────────
            // NIC formula: TotInvVal = Sum(TotItemVal) + ValDtls.OthChrg - ValDtls.Discount + RndOffAmt
            var sumTotItemVal = data.Details.Sum(x => x.TotalAmount);
            var expectedTotInvVal = sumTotItemVal + data.OtherCharges - data.Discount + data.RoundOff;
            if (Math.Abs(data.InvoiceAmount - expectedTotInvVal) > 0.01m)
                errors.Add($"Total Invoice Value ({data.InvoiceAmount}) does not match NIC formula: " +
                           $"Sum(TotItemVal)({sumTotItemVal}) + OthChrg({data.OtherCharges}) - Discount({data.Discount}) + RndOff({data.RoundOff}) = {expectedTotInvVal}.");

            // ── Seller/Buyer address mandatory fields ───────────────────────
            if (string.IsNullOrWhiteSpace(data.SellerAddr1))
                errors.Add("Seller Address Line 1 is missing.");
            if (string.IsNullOrWhiteSpace(data.SellerPinCode) || data.SellerPinCode.Length != 6)
                errors.Add($"Seller PinCode '{data.SellerPinCode}' must be exactly 6 digits.");
            if (string.IsNullOrWhiteSpace(data.BuyerAddr1))
                errors.Add("Buyer Address Line 1 is missing.");
            if (string.IsNullOrWhiteSpace(data.BuyerPinCode) || data.BuyerPinCode.Length != 6)
                errors.Add($"Buyer PinCode '{data.BuyerPinCode}' must be exactly 6 digits.");

            // ── Detail-level: at least one item required ────────────────────
            if (data.Details.Count == 0)
                errors.Add("At least one item line is required.");

            // ── Detail-level: TotItemVal consistency ────────────────────────
            foreach (var detail in data.Details)
            {
                var expectedItemVal = detail.TaxableAmount + detail.IGST + detail.CGST + detail.SGST
                    + detail.CessAmount;
                if (Math.Abs(detail.TotalAmount - expectedItemVal) > 0.01m)
                    errors.Add($"Item '{detail.ItemName}' (Line {detail.ItemSno}): TotItemVal ({detail.TotalAmount}) " +
                               $"does not match AssAmt({detail.TaxableAmount}) + Tax({detail.IGST + detail.CGST + detail.SGST}) + Cess({detail.CessAmount}) = {expectedItemVal}.");
            }

            return errors;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Step 1 — Authenticate
        // ─────────────────────────────────────────────────────────────────────

        private async Task<(string authToken, byte[] sek)> GetAuthTokenAsync(CancellationToken ct)
        {
            var cfg = GetConfig();
            var client = _httpClientFactory.CreateClient("NicEInvoice");

            var authRequest = new
            {
                UserName = cfg.UserName,
                Password = cfg.Password,
                AppKey = cfg.AppKey,
                ForceRefreshAccessToken = "Yes"
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, cfg.AuthPath)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(authRequest, _jsonOptions),
                    Encoding.UTF8, "application/json")
            };
            req.Headers.Add("client-id", cfg.ClientId);
            req.Headers.Add("client-secret", cfg.ClientSecret);
            req.Headers.Add("gstin", cfg.GstinForAuth);

            using var resp = await client.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.GetProperty("status").GetInt32() != 1)
            {
                var msg = root.TryGetProperty("message", out var m) ? m.GetString() : "Auth failed";
                throw new InvalidOperationException($"NIC auth failed: {msg}");
            }

            var data = root.GetProperty("data");
            var authToken = data.GetProperty("AuthToken").GetString()
                ?? throw new InvalidOperationException("AuthToken missing in NIC response.");
            var sekBase64 = data.GetProperty("Sek").GetString()
                ?? throw new InvalidOperationException("Sek missing in NIC response.");

            // Decrypt Sek using AppKey (Base64-decoded → 32 bytes = AES-256 key)
            var appKeyBytes = Convert.FromBase64String(cfg.AppKey);
            var sekBytes = AesDecryptEcb(sekBase64, appKeyBytes);

            return (authToken, sekBytes);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Step 2 — Load data from DB
        // ─────────────────────────────────────────────────────────────────────

        private async Task<NicEInvoiceData> LoadNicDataAsync(int headerId, CancellationToken ct)
        {
            // ── 2a. EInvoiceHeader (Finance schema) ──────────────────────────
            const string headerSql = @"
                SELECT h.Id, h.UnitId, h.PartyId, h.InvoiceNo, h.InvoiceDate,
                       h.DocType, h.SupplyType, h.PlaceOfSupply, h.GstNo,
                       h.ReverseCharge, h.CGST, h.SGST, h.IGST, h.Cess,
                       h.StateCess, h.Discount, h.OtherCharges, h.RoundOff,
                       h.InvoiceAmount
                FROM Finance.EInvoiceHeader h
                WHERE h.Id = @id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<EInvoiceHeaderRow>(
                headerSql, new { id = headerId });

            if (header is null)
                throw new InvalidOperationException(
                    $"EInvoiceHeader {headerId} not found or has been deleted.");

            // ── 2b. EInvoiceDetail rows (Finance schema) ─────────────────────
            const string detailSql = @"
                SELECT d.ItemSno, d.ItemName, d.HsnNo, d.IsService,
                       d.Qty, d.UOM, d.UnitPrice, d.GrossAmount,
                       d.Discount, d.TaxableAmount, d.GstPercentage,
                       d.CGST, d.SGST, d.IGST,
                       d.CessRate, d.CessAmount, d.TotalAmount
                FROM Finance.EInvoiceDetail d
                WHERE d.EInvoiceHeaderId = @id
                ORDER BY d.ItemSno";

            var details = (await _dbConnection.QueryAsync<EInvoiceDetailRow>(
                detailSql, new { id = headerId })).AsList();

            // ── 2c. Unit trade name (via lookup) ──────────────────────────────
            var unitDto = await _unitLookup.GetByIdAsync(header.UnitId);

            // ── 2d. Company GSTIN + legal name (AppData schema) ──────────────
            //        First get CompanyId from Unit, then query Company.
            const string unitCompanySql =
                "SELECT CompanyId FROM AppData.Unit WHERE Id = @unitId AND IsDeleted = 0";
            var unitRow = await _dbConnection.QueryFirstOrDefaultAsync<UnitCompanyRow>(
                unitCompanySql, new { unitId = header.UnitId });

            CompanyRow? companyRow = null;
            if (unitRow is not null)
            {
                const string companySql =
                    "SELECT CompanyName, GstNumber FROM AppData.Company WHERE CompanyId = @companyId";
                companyRow = await _dbConnection.QueryFirstOrDefaultAsync<CompanyRow>(
                    companySql, new { companyId = unitRow.CompanyId });
            }

            // ── 2e. Unit address (AppData schema) ────────────────────────────
            const string unitAddrSql = @"
                SELECT TOP 1 AddressLine1, AddressLine2, City, PinCode, Phone, Email
                FROM AppData.UnitAddress
                WHERE UnitId = @unitId AND IsDeleted = 0
                ORDER BY Id";
            var unitAddr = await _dbConnection.QueryFirstOrDefaultAsync<UnitAddressRow>(
                unitAddrSql, new { unitId = header.UnitId });

            // ── 2f. Buyer legal name (via lookup) ─────────────────────────────
            var partyDto = await _partyLookup.GetByIdAsync(header.PartyId);

            // ── 2g. Buyer address (PartyManagement schema) ────────────────────
            const string partyAddrSql = @"
                SELECT TOP 1 AddressLine1, AddressLine2, City, PostalCode, Phone, Email
                FROM PartyManagement.PartyAddress
                WHERE PartyId = @partyId AND IsDeleted = 0
                ORDER BY Id";
            var partyAddr = await _dbConnection.QueryFirstOrDefaultAsync<PartyAddressRow>(
                partyAddrSql, new { partyId = header.PartyId });

            // ── Assemble ──────────────────────────────────────────────────────
            return new NicEInvoiceData
            {
                Id = header.Id,
                UnitId = header.UnitId,
                PartyId = header.PartyId,
                InvoiceNo = header.InvoiceNo,
                InvoiceDate = header.InvoiceDate,
                DocType = header.DocType,
                SupplyType = header.SupplyType,
                PlaceOfSupply = header.PlaceOfSupply,
                GstNo = header.GstNo,
                ReverseCharge = header.ReverseCharge,
                CGST = header.CGST,
                SGST = header.SGST,
                IGST = header.IGST,
                Cess = header.Cess,
                StateCess = header.StateCess,
                Discount = header.Discount,
                OtherCharges = header.OtherCharges,
                RoundOff = header.RoundOff,
                InvoiceAmount = header.InvoiceAmount,

                UnitTradeName = unitDto?.UnitName,
                CompanyGstin = companyRow?.GstNumber,
                CompanyLegalName = companyRow?.CompanyName,
                SellerAddr1 = unitAddr?.AddressLine1,
                SellerAddr2 = unitAddr?.AddressLine2,
                SellerLocation = unitAddr?.City,
                SellerPinCode = unitAddr?.PinCode,
                SellerPhone = unitAddr?.Phone,
                SellerEmail = unitAddr?.Email,

                BuyerLegalName = partyDto?.PartyName,
                BuyerTradeName = partyDto?.PartyName,
                BuyerAddr1 = partyAddr?.AddressLine1,
                BuyerAddr2 = partyAddr?.AddressLine2,
                BuyerLocation = partyAddr?.City,
                BuyerPinCode = partyAddr?.PostalCode,
                BuyerPhone = partyAddr?.Phone,
                BuyerEmail = partyAddr?.Email,

                Details = details.Select(d => new NicDetailData
                {
                    ItemSno = d.ItemSno,
                    ItemName = d.ItemName,
                    HsnNo = d.HsnNo,
                    IsService = d.IsService ?? "N",
                    Qty = d.Qty,
                    UOM = d.UOM,
                    UnitPrice = d.UnitPrice,
                    GrossAmount = d.GrossAmount,
                    Discount = d.Discount,
                    TaxableAmount = d.TaxableAmount,
                    GstPercentage = d.GstPercentage,
                    CGST = d.CGST,
                    SGST = d.SGST,
                    IGST = d.IGST,
                    CessRate = d.CessRate,
                    CessAmount = d.CessAmount,
                    TotalAmount = d.TotalAmount
                }).ToList()
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Step 3 — Build NIC v1.04 JSON payload
        // ─────────────────────────────────────────────────────────────────────

        private static object BuildNicPayload(NicEInvoiceData d, EwbTransportDetails? ewb = null)
        {
            var sellerStateCode = d.CompanyGstin?.Length >= 2
                ? d.CompanyGstin.Substring(0, 2) : string.Empty;
            var buyerStateCode = d.GstNo?.Length >= 2
                ? d.GstNo.Substring(0, 2) : d.PlaceOfSupply ?? string.Empty;

            // NIC DocDtls.No: max 16 chars, pattern ^([a-zA-Z1-9]{1}[a-zA-Z0-9/-]{0,15})$
            var docNo = d.InvoiceNo ?? "INV001";
            if (docNo.Length > 16)
                docNo = docNo.Substring(docNo.Length - 16);

            var itemList = d.Details.Select(item => new
            {
                SlNo = item.ItemSno.ToString(),
                PrdDesc = item.ItemName,
                IsServc = item.IsService,
                HsnCd = item.HsnNo,
                Qty = item.Qty,
                FreeQty = 0m,
                Unit = item.UOM,
                UnitPrice = item.UnitPrice,
                TotAmt = item.GrossAmount,
                Discount = item.Discount,
                PreTaxVal = 0m,
                AssAmt = item.TaxableAmount,
                GstRt = item.GstPercentage,
                IgstAmt = item.IGST,
                CgstAmt = item.CGST,
                SgstAmt = item.SGST,
                CesRt = item.CessRate,
                CesAmt = item.CessAmount,
                CesNonAdvlAmt = 0m,
                StateCesRt = 0m,
                StateCesAmt = 0m,
                StateCesNonAdvlAmt = 0m,
                OthChrg = 0m,
                TotItemVal = item.TotalAmount
            }).ToList();

            var payload = new Dictionary<string, object>
            {
                ["Version"] = "1.1",
                ["TranDtls"] = new
                {
                    TaxSch = "GST",
                    SupTyp = d.SupplyType ?? "B2B",
                    RegRev = d.ReverseCharge ? "Y" : "N",
                    IgstOnIntra = "N"
                },
                ["DocDtls"] = new
                {
                    Typ = d.DocType ?? "INV",
                    No = docNo,
                    Dt = d.InvoiceDate.ToString("dd/MM/yyyy")
                },
                ["SellerDtls"] = new
                {
                    Gstin = d.CompanyGstin,
                    LglNm = d.CompanyLegalName,
                    TrdNm = d.UnitTradeName,
                    Addr1 = d.SellerAddr1 ?? "Address not available",
                    Addr2 = d.SellerAddr2,
                    Loc = d.SellerLocation ?? "Location",
                    Pin = int.TryParse(d.SellerPinCode, out var sPin) ? sPin : 100000,
                    Stcd = sellerStateCode,
                    Ph = d.SellerPhone,
                    Em = d.SellerEmail
                },
                ["BuyerDtls"] = new
                {
                    Gstin = d.GstNo,
                    LglNm = d.BuyerLegalName,
                    TrdNm = d.BuyerTradeName,
                    Pos = d.PlaceOfSupply ?? buyerStateCode,
                    Addr1 = d.BuyerAddr1 ?? "Address not available",
                    Addr2 = d.BuyerAddr2,
                    Loc = d.BuyerLocation ?? "Location",
                    Pin = int.TryParse(d.BuyerPinCode, out var bPin) ? bPin : 100000,
                    Stcd = buyerStateCode,
                    Ph = d.BuyerPhone,
                    Em = d.BuyerEmail
                },
                ["ItemList"] = itemList,
                ["ValDtls"] = new
                {
                    AssVal = d.Details.Sum(x => x.TaxableAmount),
                    IgstVal = d.IGST,
                    CgstVal = d.CGST,
                    SgstVal = d.SGST,
                    CesVal = d.Cess,
                    StCesVal = d.StateCess,
                    Discount = d.Discount,
                    OthChrg = d.OtherCharges,
                    RndOffAmt = d.RoundOff,
                    TotInvVal = d.InvoiceAmount
                }
            };

            // Case 1: Include EwbDtls when transport details are provided
            // NIC generates both IRN + e-Waybill in a single call
            if (ewb is not null && ewb.Distance > 0)
            {
                payload["EwbDtls"] = new
                {
                    TransId = ewb.TransId,
                    TransName = ewb.TransName,
                    Distance = ewb.Distance,
                    TransDocNo = ewb.TransDocNo,
                    TransDocDt = ewb.TransDocDt,
                    VehNo = ewb.VehNo,
                    VehType = ewb.VehType ?? "R",
                    TransMode = ewb.TransMode ?? "1"
                };
            }

            return payload;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Step 4 — Call NIC Generate IRN endpoint
        // ─────────────────────────────────────────────────────────────────────

        private async Task<NicIrnResultDto> CallGenerateIrnAsync(
            string encryptedPayload, string authToken, byte[] sek, CancellationToken ct)
        {
            var cfg = GetConfig();
            var client = _httpClientFactory.CreateClient("NicEInvoice");

            var body = JsonSerializer.Serialize(new { data = encryptedPayload }, _jsonOptions);

            using var req = new HttpRequestMessage(HttpMethod.Post, cfg.GenerateIrnPath)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            req.Headers.Add("client-id", cfg.ClientId);
            req.Headers.Add("user_name", cfg.UserName);
            req.Headers.Add("authtoken", authToken);
            req.Headers.Add("gstin", cfg.GstinForAuth);

            using var resp = await client.SendAsync(req, ct);
            var responseBody = await resp.Content.ReadAsStringAsync(ct);

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.GetProperty("status").GetInt32() != 1)
            {
                // Extract error details from NIC error response
                var message = root.TryGetProperty("message", out var msg)
                    ? msg.GetString() : "Unknown error";
                var errorCode = root.TryGetProperty("errorDetails", out var errArr)
                    && errArr.ValueKind == JsonValueKind.Array
                    && errArr.GetArrayLength() > 0
                    ? errArr[0].TryGetProperty("error_code", out var ec) ? ec.GetString() : null
                    : null;

                return new NicIrnResultDto
                {
                    IsSuccess = false,
                    ErrorCode = errorCode,
                    ErrorMessage = message
                };
            }

            // Decrypt the response data using Sek
            var encryptedResponse = root.GetProperty("data").GetString()
                ?? throw new InvalidOperationException("NIC response missing 'data' field.");

            var decryptedJson = Encoding.UTF8.GetString(AesDecryptEcb(encryptedResponse, sek));
            using var irnDoc = JsonDocument.Parse(decryptedJson);
            var irnRoot = irnDoc.RootElement;

            // Parse AckDt: "2026-03-13 10:30:00" → DateTimeOffset
            DateTimeOffset? ackDate = null;
            if (irnRoot.TryGetProperty("AckDt", out var ackDtEl) && ackDtEl.ValueKind != JsonValueKind.Null)
            {
                if (DateTimeOffset.TryParse(ackDtEl.GetString(), out var parsed))
                    ackDate = parsed;
            }

            return new NicIrnResultDto
            {
                IsSuccess = true,
                Irn = irnRoot.TryGetProperty("Irn", out var irn) ? irn.GetString() : null,
                AckNo = irnRoot.TryGetProperty("AckNo", out var ackNo)
                    ? ackNo.GetRawText().Trim('"') : null,
                AckDate = ackDate,
                SignedInvoice = irnRoot.TryGetProperty("SignedInvoice", out var si)
                    ? si.GetString() : null,
                SignedQRCode = irnRoot.TryGetProperty("SignedQRCode", out var sqr)
                    ? sqr.GetString() : null,
                // e-Waybill fields (populated only when EwbDtls was in the request)
                EwbNo = irnRoot.TryGetProperty("EwbNo", out var ewbNo)
                    && ewbNo.ValueKind == JsonValueKind.Number ? ewbNo.GetInt64() : null,
                EwbDate = irnRoot.TryGetProperty("EwbDt", out var ewbDt)
                    ? ewbDt.GetString() : null,
                EwbValidTill = irnRoot.TryGetProperty("EwbValidTill", out var ewbVt)
                    ? ewbVt.GetString() : null
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Step 5 — Call NIC e-Waybill endpoint
        // ─────────────────────────────────────────────────────────────────────

        private async Task<NicEwbResultDto> CallGenerateEwbAsync(
            string encryptedPayload, string authToken, byte[] sek, CancellationToken ct)
        {
            var cfg = GetConfig();
            var client = _httpClientFactory.CreateClient("NicEInvoice");

            var body = JsonSerializer.Serialize(new { data = encryptedPayload }, _jsonOptions);

            var ewbPath = cfg.BaseUrl +
                (_configuration.GetSection("NicEInvoice")["GenerateEwbPath"]
                    ?? "/eiewb/v1.03/ewaybill");

            using var req = new HttpRequestMessage(HttpMethod.Post, ewbPath)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            req.Headers.Add("client-id", cfg.ClientId);
            req.Headers.Add("user_name", cfg.UserName);
            req.Headers.Add("authtoken", authToken);
            req.Headers.Add("gstin", cfg.GstinForAuth);

            using var resp = await client.SendAsync(req, ct);
            var responseBody = await resp.Content.ReadAsStringAsync(ct);

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.GetProperty("status").GetInt32() != 1)
            {
                var message = root.TryGetProperty("message", out var msg)
                    ? msg.GetString() : "Unknown error";
                var errorCode = root.TryGetProperty("errorDetails", out var errArr)
                    && errArr.ValueKind == JsonValueKind.Array
                    && errArr.GetArrayLength() > 0
                    ? errArr[0].TryGetProperty("error_code", out var ec) ? ec.GetString() : null
                    : null;

                return new NicEwbResultDto
                {
                    IsSuccess = false,
                    ErrorCode = errorCode,
                    ErrorMessage = message
                };
            }

            // Decrypt the response data using Sek
            var encryptedResponse = root.GetProperty("data").GetString()
                ?? throw new InvalidOperationException("NIC e-Waybill response missing 'data' field.");

            var decryptedJson = Encoding.UTF8.GetString(AesDecryptEcb(encryptedResponse, sek));
            using var ewbDoc = JsonDocument.Parse(decryptedJson);
            var ewbRoot = ewbDoc.RootElement;

            return new NicEwbResultDto
            {
                IsSuccess = true,
                EwbNo = ewbRoot.TryGetProperty("EwbNo", out var ewbNo)
                    ? ewbNo.GetInt64() : null,
                EwbDate = ewbRoot.TryGetProperty("EwbDt", out var ewbDt)
                    ? ewbDt.GetString() : null,
                EwbValidTill = ewbRoot.TryGetProperty("EwbValidTill", out var ewbValid)
                    ? ewbValid.GetString() : null
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  AES-256 ECB helpers (NIC uses ECB mode, no IV)
        // ─────────────────────────────────────────────────────────────────────

        private static byte[] AesDecryptEcb(string base64Cipher, byte[] key)
        {
            var cipher = Convert.FromBase64String(base64Cipher);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            using var dec = aes.CreateDecryptor();
            return dec.TransformFinalBlock(cipher, 0, cipher.Length);
        }

        private static string AesEncryptEcb(string plainText, byte[] key)
        {
            var plain = Encoding.UTF8.GetBytes(plainText);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            using var enc = aes.CreateEncryptor();
            var cipher = enc.TransformFinalBlock(plain, 0, plain.Length);
            return Convert.ToBase64String(cipher);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Configuration helper
        // ─────────────────────────────────────────────────────────────────────

        private NicEInvoiceConfig GetConfig()
        {
            var section = _configuration.GetSection("NicEInvoice");
            var env = section["Environment"] ?? "Sandbox";
            var baseUrl = env == "Production"
                ? (section["ProductionBaseUrl"] ?? "https://einvoice1.gst.gov.in")
                : (section["SandboxBaseUrl"] ?? "https://einv-apisandbox.nic.in");

            return new NicEInvoiceConfig
            {
                BaseUrl = baseUrl,
                AuthPath = baseUrl + (section["AuthPath"] ?? "/eivital/v1.04/auth"),
                GenerateIrnPath = baseUrl + (section["GenerateIrnPath"] ?? "/eicore/v1.03/Invoice"),
                ClientId = section["ClientId"] ?? string.Empty,
                ClientSecret = section["ClientSecret"] ?? string.Empty,
                AppKey = section["AppKey"] ?? string.Empty,
                UserName = section["UserName"] ?? string.Empty,
                Password = section["Password"] ?? string.Empty,
                GstinForAuth = section["GstinForAuth"] ?? string.Empty
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Internal data models (Dapper row mappers + NIC data carrier)
        // ─────────────────────────────────────────────────────────────────────

        private sealed class NicEInvoiceConfig
        {
            public string BaseUrl { get; set; } = string.Empty;
            public string AuthPath { get; set; } = string.Empty;
            public string GenerateIrnPath { get; set; } = string.Empty;
            public string ClientId { get; set; } = string.Empty;
            public string ClientSecret { get; set; } = string.Empty;
            public string AppKey { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string GstinForAuth { get; set; } = string.Empty;
        }

        private sealed class EInvoiceHeaderRow
        {
            public int Id { get; set; }
            public int UnitId { get; set; }
            public int PartyId { get; set; }
            public string? InvoiceNo { get; set; }
            public DateOnly InvoiceDate { get; set; }
            public string? DocType { get; set; }
            public string? SupplyType { get; set; }
            public string? PlaceOfSupply { get; set; }
            public string? GstNo { get; set; }
            public bool ReverseCharge { get; set; }
            public decimal CGST { get; set; }
            public decimal SGST { get; set; }
            public decimal IGST { get; set; }
            public decimal Cess { get; set; }
            public decimal StateCess { get; set; }
            public decimal Discount { get; set; }
            public decimal OtherCharges { get; set; }
            public decimal RoundOff { get; set; }
            public decimal InvoiceAmount { get; set; }
        }

        private sealed class EInvoiceDetailRow
        {
            public int ItemSno { get; set; }
            public string? ItemName { get; set; }
            public string? HsnNo { get; set; }
            public string? IsService { get; set; }
            public decimal Qty { get; set; }
            public string? UOM { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal GrossAmount { get; set; }
            public decimal Discount { get; set; }
            public decimal TaxableAmount { get; set; }
            public decimal GstPercentage { get; set; }
            public decimal CGST { get; set; }
            public decimal SGST { get; set; }
            public decimal IGST { get; set; }
            public decimal CessRate { get; set; }
            public decimal CessAmount { get; set; }
            public decimal TotalAmount { get; set; }
        }

        private sealed class UnitCompanyRow { public int CompanyId { get; set; } }
        private sealed class CompanyRow { public string? CompanyName { get; set; } public string? GstNumber { get; set; } }
        private sealed class UnitAddressRow { public string? AddressLine1 { get; set; } public string? AddressLine2 { get; set; } public string? City { get; set; } public string? PinCode { get; set; } public string? Phone { get; set; } public string? Email { get; set; } }
        private sealed class PartyAddressRow { public string? AddressLine1 { get; set; } public string? AddressLine2 { get; set; } public string? City { get; set; } public string? PostalCode { get; set; } public string? Phone { get; set; } public string? Email { get; set; } }

        private sealed class NicEInvoiceData
        {
            public int Id { get; set; }
            public int UnitId { get; set; }
            public int PartyId { get; set; }
            public string? InvoiceNo { get; set; }
            public DateOnly InvoiceDate { get; set; }
            public string? DocType { get; set; }
            public string? SupplyType { get; set; }
            public string? PlaceOfSupply { get; set; }
            public string? GstNo { get; set; }
            public bool ReverseCharge { get; set; }
            public decimal CGST { get; set; }
            public decimal SGST { get; set; }
            public decimal IGST { get; set; }
            public decimal Cess { get; set; }
            public decimal StateCess { get; set; }
            public decimal Discount { get; set; }
            public decimal OtherCharges { get; set; }
            public decimal RoundOff { get; set; }
            public decimal InvoiceAmount { get; set; }
            public string? UnitTradeName { get; set; }
            public string? CompanyGstin { get; set; }
            public string? CompanyLegalName { get; set; }
            public string? SellerAddr1 { get; set; }
            public string? SellerAddr2 { get; set; }
            public string? SellerLocation { get; set; }
            public string? SellerPinCode { get; set; }
            public string? SellerPhone { get; set; }
            public string? SellerEmail { get; set; }
            public string? BuyerLegalName { get; set; }
            public string? BuyerTradeName { get; set; }
            public string? BuyerAddr1 { get; set; }
            public string? BuyerAddr2 { get; set; }
            public string? BuyerLocation { get; set; }
            public string? BuyerPinCode { get; set; }
            public string? BuyerPhone { get; set; }
            public string? BuyerEmail { get; set; }
            public List<NicDetailData> Details { get; set; } = new();
        }

        private sealed class NicDetailData
        {
            public int ItemSno { get; set; }
            public string? ItemName { get; set; }
            public string? HsnNo { get; set; }
            public string? IsService { get; set; }
            public decimal Qty { get; set; }
            public string? UOM { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal GrossAmount { get; set; }
            public decimal Discount { get; set; }
            public decimal TaxableAmount { get; set; }
            public decimal GstPercentage { get; set; }
            public decimal CGST { get; set; }
            public decimal SGST { get; set; }
            public decimal IGST { get; set; }
            public decimal CessRate { get; set; }
            public decimal CessAmount { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
