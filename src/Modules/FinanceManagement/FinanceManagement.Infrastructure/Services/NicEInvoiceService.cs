using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts.Common;
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
        private readonly ICityLookup _cityLookup;
        private readonly IConfiguration _configuration;

        // ── JSON serialiser options (camelCase disabled; NIC uses PascalCase) ──
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        // ── NIC-valid Unit Quantity Codes (UQC) ─────────────────────────────
        // Reference: NIC e-Invoice API v1.04 — UQC Master
        private static readonly HashSet<string> _validUqcCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            "BAG", "BAL", "BDL", "BKL", "BOU", "BOX", "BTL", "BUN", "CAN",
            "CBM", "CCM", "CMS", "CTN", "DOZ", "DRM", "GGK", "GMS", "GRS",
            "GYD", "KGS", "KLR", "KME", "LTR", "MLS", "MLT", "MTR", "MTS",
            "NOS", "OTH", "PAC", "PCS", "PRS", "QTL", "ROL", "SET", "SQF",
            "SQM", "SQY", "TBS", "TGM", "THD", "TON", "TUB", "UGS", "UNT",
            "YDS"
        };

        // ── Common UOM name/code → NIC UQC mapping ─────────────────────────
        // Maps DB values (Inventory.UOM.Code or UOMName) that don't match NIC
        // codes directly to their correct NIC UQC equivalent.
        private static readonly Dictionary<string, string> _uqcMapping =
            new(StringComparer.OrdinalIgnoreCase)
        {
            // Weight (OrdinalIgnoreCase — one entry per unique key)
            { "Kg", "KGS" }, { "Kgs", "KGS" },
            { "Kilogram", "KGS" }, { "Kilograms", "KGS" },
            { "Gm", "GMS" }, { "Gram", "GMS" }, { "Grams", "GMS" },
            { "Quintal", "QTL" }, { "Quintals", "QTL" },
            { "Ton", "TON" }, { "Tonne", "TON" }, { "Tonnes", "TON" },
            { "MT", "MTS" }, { "MetricTon", "MTS" }, { "MetricTonne", "MTS" },

            // Count
            { "No", "NOS" }, { "Nos", "NOS" }, { "Number", "NOS" }, { "Numbers", "NOS" },
            { "Pc", "PCS" }, { "Pcs", "PCS" }, { "Piece", "PCS" }, { "Pieces", "PCS" },
            { "Pair", "PRS" }, { "Pairs", "PRS" },
            { "Sets", "SET" },
            { "Dozen", "DOZ" }, { "Dzn", "DOZ" },
            { "Units", "UNT" },
            { "Each", "NOS" },

            // Volume
            { "Ltr", "LTR" }, { "Litre", "LTR" }, { "Liter", "LTR" }, { "Litres", "LTR" },
            { "Ml", "MLS" }, { "Millilitre", "MLS" },

            // Length
            { "Mtr", "MTR" }, { "Meter", "MTR" }, { "Metre", "MTR" }, { "Meters", "MTR" },
            { "Cm", "CMS" }, { "Centimeter", "CMS" },
            { "Yard", "YDS" }, { "Yards", "YDS" },

            // Area
            { "SqMtr", "SQM" }, { "SqM", "SQM" }, { "SquareMeter", "SQM" },
            { "SqFt", "SQF" }, { "SquareFeet", "SQF" }, { "SquareFoot", "SQF" },

            // Packaging
            { "Bags", "BAG" },
            { "Boxes", "BOX" },
            { "Bale", "BAL" }, { "Bales", "BAL" },
            { "Bundle", "BDL" }, { "Bundles", "BDL" },
            { "Bottle", "BTL" }, { "Bottles", "BTL" },
            { "Pack", "PAC" }, { "Packs", "PAC" }, { "Packet", "PAC" },
            { "Rolls", "ROL" },
            { "Drums", "DRM" },
            { "Cartons", "CTN" },
            { "Cans", "CAN" },
            { "Tubes", "TUB" },

            // Others
            { "Gross", "GRS" },
            { "Buckle", "BKL" },
            { "Other", "OTH" }, { "Others", "OTH" },
        };

        // ── NIC-notified GST rates ──────────────────────────────────────────
        private static readonly decimal[] _notifiedGstRates =
            { 0m, 0.1m, 0.25m, 1m, 1.5m, 3m, 5m, 7.5m, 12m, 18m, 28m };

        /// <summary>
        /// Resolves a UOM value from the database to a valid NIC UQC code.
        /// Returns the NIC code if already valid, maps common names/abbreviations,
        /// or falls back to "OTH" if no match is found.
        /// </summary>
        private static string ResolveUqcCode(string? uom)
        {
            if (string.IsNullOrWhiteSpace(uom))
                return "OTH";

            var trimmed = uom.Trim();

            // Already a valid NIC UQC code
            if (_validUqcCodes.Contains(trimmed))
                return trimmed.ToUpperInvariant();

            // Try mapping common names/abbreviations
            if (_uqcMapping.TryGetValue(trimmed, out var mapped))
                return mapped;

            // Fallback
            return "OTH";
        }

        /// <summary>
        /// Returns true if the value matches a NIC-notified GST rate (within 0.01 tolerance).
        /// </summary>
        private static bool IsNotifiedRate(decimal rate)
        {
            var rounded = Math.Round(rate, 2);
            foreach (var r in _notifiedGstRates)
            {
                if (Math.Abs(rounded - r) <= 0.01m)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Snaps a GST percentage to the nearest NIC-notified rate.
        /// If the stored GstPercentage is not a notified rate, computes the effective rate
        /// from the actual CGST+SGST+IGST amounts relative to TaxableAmount.
        /// </summary>
        private static decimal ResolveGstRate(decimal gstPercentage, decimal taxableAmount,
            decimal cgst, decimal sgst, decimal igst)
        {
            // 1. Try the stored percentage first
            var rounded = Math.Round(gstPercentage, 2);
            foreach (var rate in _notifiedGstRates)
            {
                if (Math.Abs(rounded - rate) <= 0.01m)
                    return rate;
            }

            // 2. Stored percentage is not a notified rate — compute from actual tax amounts
            if (taxableAmount > 0)
            {
                var totalTax = cgst + sgst + igst;
                var effectiveRate = Math.Round(totalTax / taxableAmount * 100m, 2);
                foreach (var rate in _notifiedGstRates)
                {
                    if (Math.Abs(effectiveRate - rate) <= 0.05m)
                        return rate;
                }
            }

            // 3. Neither matched — return stored value rounded (pre-flight validation will warn)
            return rounded;
        }

        public NicEInvoiceService(
            IHttpClientFactory httpClientFactory,
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IPartyLookup partyLookup,
            ICityLookup cityLookup,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _partyLookup = partyLookup;
            _cityLookup = cityLookup;
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
                // Include method name from stack trace for debugging
                var source = ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim() ?? "unknown";
                return new NicIrnResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "SERVICE_ERROR",
                    ErrorMessage = $"{ex.Message} | Source: {source}"
                };
            }
        }

        public async Task<NicCancelIrnResultDto> CancelIrnAsync(
            int eInvoiceHeaderId,
            string cnlRsn,
            string? cnlRem = null,
            CancellationToken ct = default)
        {
            try
            {
                // 1. Load IRN from EInvoiceHeader
                const string irnSql = @"
                    SELECT IrnNumber
                    FROM Finance.EInvoiceHeader
                    WHERE Id = @id AND IsDeleted = 0";

                var irnNumber = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                    irnSql, new { id = eInvoiceHeaderId });

                if (string.IsNullOrEmpty(irnNumber))
                    return new NicCancelIrnResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "VALIDATION_ERROR",
                        ErrorMessage = $"EInvoiceHeader {eInvoiceHeaderId} has no IRN. Cannot cancel."
                    };

                // 2. Authenticate with NIC
                var (authToken, sek) = await GetAuthTokenAsync(ct);

                // 3. Build cancel payload
                var cancelPayload = new
                {
                    Irn = irnNumber,
                    CnlRsn = cnlRsn,
                    CnlRem = cnlRem ?? "Cancelled"
                };

                var jsonString = JsonSerializer.Serialize(cancelPayload, _jsonOptions);
                var encryptedPayload = AesEncryptEcb(jsonString, sek);

                // 4. Call NIC Cancel IRN API
                var cfg = GetConfig();
                var client = _httpClientFactory.CreateClient("NicEInvoice");
                var cancelPath = cfg.BaseUrl +
                    (_configuration.GetSection("NicEInvoice")["CancelIrnPath"]
                        ?? "/eicore/v1.03/Invoice/Cancel");

                var body = JsonSerializer.Serialize(new { Data = encryptedPayload }, _jsonOptions);

                using var req = new HttpRequestMessage(HttpMethod.Post, cancelPath)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                req.Headers.Add("client-id", cfg.ClientId);
                req.Headers.Add("client-secret", cfg.ClientSecret);
                req.Headers.Add("user_name", cfg.UserName);
                req.Headers.Add("authtoken", authToken);
                req.Headers.Add("gstin", cfg.GstinForAuth);

                using var resp = await client.SendAsync(req, ct);
                var responseBody = await resp.Content.ReadAsStringAsync(ct);

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                // Check status (NIC returns Status/status)
                var statusOk = false;
                if (root.TryGetProperty("Status", out var statusProp) || root.TryGetProperty("status", out statusProp))
                {
                    var statusVal = statusProp.ValueKind == JsonValueKind.Number
                        ? statusProp.GetInt32()
                        : int.TryParse(statusProp.GetString(), out var sv) ? sv : 0;
                    statusOk = statusVal == 1;
                }

                if (!statusOk)
                {
                    var message = root.TryGetProperty("ErrorDetails", out var ed) ? ed.GetRawText()
                        : root.TryGetProperty("errorDetails", out ed) ? ed.GetRawText()
                        : responseBody.Length > 500 ? responseBody[..500] : responseBody;

                    return new NicCancelIrnResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "NIC_ERROR",
                        ErrorMessage = message
                    };
                }

                // Decrypt response
                var encData = root.TryGetProperty("Data", out var dataProp) ? dataProp.GetString()
                    : root.TryGetProperty("data", out dataProp) ? dataProp.GetString()
                    : null;

                if (string.IsNullOrEmpty(encData))
                    return new NicCancelIrnResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "NIC_ERROR",
                        ErrorMessage = "NIC cancel response missing 'Data' field."
                    };

                var decryptedJson = Encoding.UTF8.GetString(AesDecryptEcb(encData, sek));
                using var cancelDoc = JsonDocument.Parse(decryptedJson);
                var cancelRoot = cancelDoc.RootElement;

                return new NicCancelIrnResultDto
                {
                    IsSuccess = true,
                    Irn = cancelRoot.TryGetProperty("Irn", out var irn) ? irn.GetString() : irnNumber,
                    CancelDate = cancelRoot.TryGetProperty("CancelDate", out var cd) ? cd.GetString()
                        : cancelRoot.TryGetProperty("CnlDt", out cd) ? cd.GetString() : null
                };
            }
            catch (Exception ex)
            {
                var source = ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim() ?? "unknown";
                return new NicCancelIrnResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "SERVICE_ERROR",
                    ErrorMessage = $"{ex.Message} | Source: {source}"
                };
            }
        }

        public async Task<ApiResponseDTO<object>> GetIrnDetailsAsync(
            int eInvoiceHeaderId, CancellationToken ct = default)
        {
            try
            {
                // 1. Load IRN from DB
                const string irnSql = @"
                    SELECT IrnNumber FROM Finance.EInvoiceHeader
                    WHERE Id = @id AND IsDeleted = 0";

                var irnNumber = await _dbConnection.QueryFirstOrDefaultAsync<string>(irnSql, new { id = eInvoiceHeaderId });

                if (string.IsNullOrEmpty(irnNumber))
                    return new ApiResponseDTO<object>
                    {
                        IsSuccess = false,
                        Message = $"EInvoiceHeader {eInvoiceHeaderId} has no IRN."
                    };

                // 2. Auth
                var (authToken, sek) = await GetAuthTokenAsync(ct);
                var cfg = GetConfig();
                var client = _httpClientFactory.CreateClient("NicEInvoice");

                // 3. GET /eicore/v1.03/Invoice/irn/{irn}
                var getIrnPath = cfg.BaseUrl + "/eicore/v1.03/Invoice/irn/" + irnNumber;

                using var req = new HttpRequestMessage(HttpMethod.Get, getIrnPath);
                req.Headers.Add("client-id", cfg.ClientId);
                req.Headers.Add("client-secret", cfg.ClientSecret);
                req.Headers.Add("user_name", cfg.UserName);
                req.Headers.Add("authtoken", authToken);
                req.Headers.Add("gstin", cfg.GstinForAuth);

                using var resp = await client.SendAsync(req, ct);
                var responseBody = await resp.Content.ReadAsStringAsync(ct);

                if (string.IsNullOrWhiteSpace(responseBody))
                    return new ApiResponseDTO<object>
                    {
                        IsSuccess = false,
                        Message = $"NIC returned empty response (HTTP {(int)resp.StatusCode})."
                    };

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                var statusOk = false;
                if (root.TryGetProperty("Status", out var sp) || root.TryGetProperty("status", out sp))
                {
                    var sv = sp.ValueKind == JsonValueKind.Number ? sp.GetInt32()
                        : int.TryParse(sp.GetString(), out var v) ? v : 0;
                    statusOk = sv == 1;
                }

                if (!statusOk)
                {
                    var message = root.TryGetProperty("ErrorDetails", out var ed) ? ed.GetRawText()
                        : root.TryGetProperty("errorDetails", out ed) ? ed.GetRawText()
                        : responseBody.Length > 500 ? responseBody[..500] : responseBody;

                    return new ApiResponseDTO<object>
                    {
                        IsSuccess = false,
                        Message = message
                    };
                }

                // 4. Decrypt response
                var encData = root.TryGetProperty("Data", out var dp) ? dp.GetString()
                    : root.TryGetProperty("data", out dp) ? dp.GetString() : null;

                if (string.IsNullOrEmpty(encData))
                    return new ApiResponseDTO<object>
                    {
                        IsSuccess = false,
                        Message = "NIC response missing 'Data' field."
                    };

                var decryptedJson = Encoding.UTF8.GetString(AesDecryptEcb(encData, sek));
                var irnDetails = JsonSerializer.Deserialize<JsonElement>(decryptedJson);

                return new ApiResponseDTO<object>
                {
                    IsSuccess = true,
                    Message = "IRN details fetched successfully.",
                    Data = irnDetails
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<object>
                {
                    IsSuccess = false,
                    Message = $"{ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDTO<object>> GetEwbDetailsByIrnAsync(
            int eInvoiceHeaderId, CancellationToken ct = default)
        {
            try
            {
                // 1. Load IRN from DB
                const string irnSql = @"
                    SELECT IrnNumber FROM Finance.EInvoiceHeader
                    WHERE Id = @id AND IsDeleted = 0";

                var irnNumber = await _dbConnection.QueryFirstOrDefaultAsync<string>(irnSql, new { id = eInvoiceHeaderId });

                if (string.IsNullOrEmpty(irnNumber))
                    return new ApiResponseDTO<object>
                    {
                        IsSuccess = false,
                        Message = $"EInvoiceHeader {eInvoiceHeaderId} has no IRN."
                    };

                // 2. Auth
                var (authToken, sek) = await GetAuthTokenAsync(ct);
                var cfg = GetConfig();
                var client = _httpClientFactory.CreateClient("NicEInvoice");

                // 3. GET /eiewb/v1.03/ewaybill/irn/{irn}
                var ewbPath = cfg.BaseUrl + "/eiewb/v1.03/ewaybill/irn/" + irnNumber;

                using var req = new HttpRequestMessage(HttpMethod.Get, ewbPath);
                req.Headers.Add("client-id", cfg.ClientId);
                req.Headers.Add("client-secret", cfg.ClientSecret);
                req.Headers.Add("user_name", cfg.UserName);
                req.Headers.Add("authtoken", authToken);
                req.Headers.Add("gstin", cfg.GstinForAuth);

                using var resp = await client.SendAsync(req, ct);
                var responseBody = await resp.Content.ReadAsStringAsync(ct);

                if (string.IsNullOrWhiteSpace(responseBody))
                    return new ApiResponseDTO<object>
                    {
                        IsSuccess = false,
                        Message = $"NIC returned empty response (HTTP {(int)resp.StatusCode})."
                    };

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                var statusOk = false;
                if (root.TryGetProperty("Status", out var sp) || root.TryGetProperty("status", out sp))
                {
                    var sv = sp.ValueKind == JsonValueKind.Number ? sp.GetInt32()
                        : int.TryParse(sp.GetString(), out var v) ? v : 0;
                    statusOk = sv == 1;
                }

                if (!statusOk)
                {
                    var message = root.TryGetProperty("ErrorDetails", out var ed) ? ed.GetRawText()
                        : root.TryGetProperty("errorDetails", out ed) ? ed.GetRawText()
                        : responseBody.Length > 500 ? responseBody[..500] : responseBody;

                    return new ApiResponseDTO<object>
                    {
                        IsSuccess = false,
                        Message = message
                    };
                }

                // 4. Decrypt response
                var encData = root.TryGetProperty("Data", out var dp) ? dp.GetString()
                    : root.TryGetProperty("data", out dp) ? dp.GetString() : null;

                if (string.IsNullOrEmpty(encData))
                    return new ApiResponseDTO<object>
                    {
                        IsSuccess = false,
                        Message = "NIC response missing 'Data' field."
                    };

                var decryptedJson = Encoding.UTF8.GetString(AesDecryptEcb(encData, sek));
                var ewbDetails = JsonSerializer.Deserialize<JsonElement>(decryptedJson);

                return new ApiResponseDTO<object>
                {
                    IsSuccess = true,
                    Message = "e-Waybill details fetched successfully.",
                    Data = ewbDetails
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<object>
                {
                    IsSuccess = false,
                    Message = $"{ex.Message}"
                };
            }
        }

        public async Task<NicCancelEwbResultDto> CancelEwbAsync(
            int eInvoiceHeaderId,
            int cancelRsnCode,
            string? cancelRmrk = null,
            CancellationToken ct = default)
        {
            try
            {
                // 1. Get EWB number from EWaybillHeader table
                const string ewbSql = @"
                    SELECT TOP 1 EWBNumber
                    FROM Finance.EWaybillHeader
                    WHERE EInvoiceHeaderId = @id AND IsDeleted = 0 AND EwbStatus = 'Generated'
                    ORDER BY Id DESC";

                var ewbNumberStr = await _dbConnection.QueryFirstOrDefaultAsync<string>(ewbSql, new { id = eInvoiceHeaderId });

                if (string.IsNullOrEmpty(ewbNumberStr) || !long.TryParse(ewbNumberStr, out var ewbNo) || ewbNo == 0)
                    return new NicCancelEwbResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "VALIDATION_ERROR",
                        ErrorMessage = $"EInvoiceHeader {eInvoiceHeaderId} has no active e-Waybill to cancel."
                    };

                // 2. Authenticate with NIC
                var (authToken, sek) = await GetAuthTokenAsync(ct);
                var cfg = GetConfig();
                var client = _httpClientFactory.CreateClient("NicEInvoice");

                // 2. Build cancel payload
                var cancelPayload = new
                {
                    ewbNo = ewbNo,
                    cancelRsnCode = cancelRsnCode,
                    cancelRmrk = cancelRmrk ?? "Cancelled"
                };

                var cancelJson = JsonSerializer.Serialize(cancelPayload, _jsonOptions);
                var encCancel = AesEncryptEcb(cancelJson, sek);

                // 3. Call NIC Cancel e-Waybill API
                // Cancel EWB uses a different host than IRN/EWB generation
                var ewbBaseUrl = _configuration.GetSection("NicEInvoice")["EwbBaseUrl"]
                    ?? "https://einv-apisandbox.nic.in";
                var cancelPath = ewbBaseUrl +
                    (_configuration.GetSection("NicEInvoice")["CancelEwbPath"]
                        ?? "/ewaybillapi/v1.03/ewayapi");

                // NIC Cancel EWB requires "action" field in the body
                var cancelBody = JsonSerializer.Serialize(new { action = "CANEWB", Data = encCancel }, _jsonOptions);

                using var cancelReq = new HttpRequestMessage(HttpMethod.Post, cancelPath)
                {
                    Content = new StringContent(cancelBody, Encoding.UTF8, "application/json")
                };
                cancelReq.Headers.Add("client-id", cfg.ClientId);
                cancelReq.Headers.Add("client-secret", cfg.ClientSecret);
                cancelReq.Headers.Add("user_name", cfg.UserName);
                cancelReq.Headers.Add("authtoken", authToken);
                cancelReq.Headers.Add("gstin", cfg.GstinForAuth);

                using var cancelResp = await client.SendAsync(cancelReq, ct);
                var cancelRespBody = await cancelResp.Content.ReadAsStringAsync(ct);

                // If empty response, return error with HTTP status
                if (string.IsNullOrWhiteSpace(cancelRespBody))
                    return new NicCancelEwbResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "NIC_ERROR",
                        ErrorMessage = $"NIC returned empty response (HTTP {(int)cancelResp.StatusCode}). URL: {cancelPath}"
                    };

                using var cancelDoc = JsonDocument.Parse(cancelRespBody);
                var cancelRoot = cancelDoc.RootElement;

                var cancelStatusOk = false;
                if (cancelRoot.TryGetProperty("Status", out var csp) || cancelRoot.TryGetProperty("status", out csp))
                {
                    var csv = csp.ValueKind == JsonValueKind.Number ? csp.GetInt32()
                        : int.TryParse(csp.GetString(), out var v) ? v : 0;
                    cancelStatusOk = csv == 1;
                }

                if (!cancelStatusOk)
                {
                    // EWB API returns error as Base64-encoded JSON in "error" field
                    var message = cancelRespBody;
                    if (cancelRoot.TryGetProperty("error", out var errProp) && errProp.ValueKind == JsonValueKind.String)
                    {
                        try
                        {
                            var errJson = Encoding.UTF8.GetString(Convert.FromBase64String(errProp.GetString()!));
                            message = errJson;
                        }
                        catch { message = errProp.GetString(); }
                    }
                    else if (cancelRoot.TryGetProperty("ErrorDetails", out var ed))
                        message = ed.GetRawText();
                    else if (cancelRoot.TryGetProperty("errorDetails", out ed))
                        message = ed.GetRawText();

                    if (message?.Length > 500) message = message[..500];

                    return new NicCancelEwbResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "NIC_ERROR",
                        ErrorMessage = message
                    };
                }

                // Decrypt response — EWB API uses "data" (lowercase)
                var encResult = cancelRoot.TryGetProperty("data", out var rdp) ? rdp.GetString()
                    : cancelRoot.TryGetProperty("Data", out rdp) ? rdp.GetString() : null;

                string? cancelDate = null;
                if (!string.IsNullOrEmpty(encResult))
                {
                    var decResult = Encoding.UTF8.GetString(AesDecryptEcb(encResult, sek));
                    using var resultDoc = JsonDocument.Parse(decResult);
                    if (resultDoc.RootElement.TryGetProperty("cancelDate", out var cdProp))
                        cancelDate = cdProp.GetString();
                    else if (resultDoc.RootElement.TryGetProperty("CancelDate", out cdProp))
                        cancelDate = cdProp.GetString();
                }

                return new NicCancelEwbResultDto
                {
                    IsSuccess = true,
                    EwbNo = ewbNo,
                    CancelDate = cancelDate
                };
            }
            catch (Exception ex)
            {
                var source = ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim() ?? "unknown";
                return new NicCancelEwbResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "SERVICE_ERROR",
                    ErrorMessage = $"{ex.Message} | Source: {source}"
                };
            }
        }

        public async Task<NicEwbResultDto> GenerateEwbAsync(
            int eInvoiceHeaderId,
            string? transporterId,
            string? transporterName,
            string? transMode,
            int distance,
            string? transDocNo,
            string? transDocDt,
            string? vehicleNo,
            string? vehicleType,
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
                // NIC TransMode must be "1"/"2"/"3"/"4". Use DB value if valid; default to "1" (Road)
                // when VehicleNo or TransDocNo is present (NIC error 4028 if omitted in that case).
                var nicTransMode = transMode is "1" or "2" or "3" or "4" ? transMode : null;
                if (nicTransMode == null && (!string.IsNullOrWhiteSpace(vehicleNo) || !string.IsNullOrWhiteSpace(transDocNo)))
                    nicTransMode = "1";

                var ewbPayload = new
                {
                    Irn = irnNumber,
                    Distance = distance,
                    TransMode = nicTransMode,
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
                var source = ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim() ?? "unknown";
                return new NicEwbResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "SERVICE_ERROR",
                    ErrorMessage = $"{ex.Message} | Source: {source}"
                };
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Standalone e-Waybill (no IRN — used by DC, Bill of Supply, etc.)
        //  POSTs to /ewaybillapi/v1.03/ewayapi with action=GENEWAYBILL.
        //  Mirrors CancelEwbAsync's auth/encrypt/decrypt pattern.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<NicEwbResultDto> GenerateStandaloneEwbAsync(
            StandaloneEwbPayloadDto payload,
            CancellationToken ct = default)
        {
            try
            {
                // 1a. Enrich address fields from UnitId hints if caller didn't fill them.
                //     Keeps DC/BoS handlers free of cross-schema data access.
                await EnrichStandalonePayloadAddressesAsync(payload, ct);

                // 1b. Derive State codes from GSTIN's first 2 chars when caller left them 0.
                if (payload.FromStateCode == 0 && payload.FromGstin?.Length >= 2
                    && int.TryParse(payload.FromGstin[..2], out var fromState))
                {
                    payload.FromStateCode = fromState;
                    if (payload.ActFromStateCode == 0) payload.ActFromStateCode = fromState;
                }
                if (payload.ToStateCode == 0 && payload.ToGstin?.Length >= 2
                    && int.TryParse(payload.ToGstin[..2], out var toState))
                {
                    payload.ToStateCode = toState;
                    if (payload.ActToStateCode == 0) payload.ActToStateCode = toState;
                }

                // 1c. Pre-flight validation
                var preflightErrors = ValidateStandalonePayload(payload);
                if (preflightErrors.Count > 0)
                    return new NicEwbResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "VALIDATION_ERROR",
                        ErrorMessage = string.Join(" | ", preflightErrors)
                    };

                // 2. Authenticate with NIC
                var (authToken, sek) = await GetAuthTokenAsync(ct);
                var cfg = GetConfig();
                var client = _httpClientFactory.CreateClient("NicEInvoice");

                // 3. Serialize + AES-encrypt payload
                var jsonString = JsonSerializer.Serialize(payload, _jsonOptions);
                var encryptedPayload = AesEncryptEcb(jsonString, sek);

                // 4. Build request — EWB API expects { action, data }
                // NOTE: standalone EWB uses a DIFFERENT path than the IRN-based EWB.
                // IRN-based:  /eiewb/v1.03/ewaybill        (config key: GenerateEwbPath, used by GenerateEwbAsync)
                // Standalone: /ewaybillapi/v1.03/ewayapi   (config key: GenerateStandaloneEwbPath — this method)
                // Reusing GenerateEwbPath here would break IRN flows or get a 404 for standalone.
                var ewbBaseUrl = _configuration.GetSection("NicEInvoice")["EwbBaseUrl"]
                    ?? "https://einv-apisandbox.nic.in";
                var generatePath = ewbBaseUrl +
                    (_configuration.GetSection("NicEInvoice")["GenerateStandaloneEwbPath"]
                        ?? "/ewaybillapi/v1.03/ewayapi");

                var body = JsonSerializer.Serialize(
                    new { action = "GENEWAYBILL", Data = encryptedPayload }, _jsonOptions);

                using var req = new HttpRequestMessage(HttpMethod.Post, generatePath)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                req.Headers.Add("client-id", cfg.ClientId);
                req.Headers.Add("client-secret", cfg.ClientSecret);
                req.Headers.Add("user_name", cfg.UserName);
                req.Headers.Add("authtoken", authToken);
                req.Headers.Add("gstin", cfg.GstinForAuth);

                // 5. Send + parse response
                using var resp = await client.SendAsync(req, ct);
                var responseBody = await resp.Content.ReadAsStringAsync(ct);

                if (string.IsNullOrWhiteSpace(responseBody))
                    return new NicEwbResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "NIC_ERROR",
                        ErrorMessage = $"NIC returned empty response (HTTP {(int)resp.StatusCode}). URL: {generatePath}"
                    };

                // Defensive: NIC sometimes returns an HTML error page (404/500/auth gate).
                // Detect non-JSON bodies BEFORE JsonDocument.Parse throws a confusing exception
                // and surface enough context (status + URL + body preview) for the operator to debug.
                var trimmed = responseBody.TrimStart();
                if (trimmed.Length == 0 || (trimmed[0] != '{' && trimmed[0] != '['))
                {
                    // Cap preview tightly so the assembled message fits within
                    // EWaybillHeader.ErrorMessage's varchar(500) column.
                    var preview = responseBody.Length > 200 ? responseBody[..200] : responseBody;
                    return new NicEwbResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "NIC_NON_JSON",          // <= 20 chars (varchar(20) column)
                        ErrorMessage = $"NIC non-JSON (HTTP {(int)resp.StatusCode}). URL: {generatePath}. Body: {preview}"
                    };
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                var statusOk = false;
                if (root.TryGetProperty("Status", out var sp) || root.TryGetProperty("status", out sp))
                {
                    var sv = sp.ValueKind == JsonValueKind.Number ? sp.GetInt32()
                        : int.TryParse(sp.GetString(), out var v) ? v : 0;
                    statusOk = sv == 1;
                }

                if (!statusOk)
                {
                    // EWB API returns error as Base64-encoded JSON in "error"
                    var message = responseBody;
                    if (root.TryGetProperty("error", out var errProp) && errProp.ValueKind == JsonValueKind.String)
                    {
                        try
                        {
                            message = Encoding.UTF8.GetString(Convert.FromBase64String(errProp.GetString()!));
                        }
                        catch { message = errProp.GetString(); }
                    }
                    else if (root.TryGetProperty("ErrorDetails", out var ed))
                        message = ed.GetRawText();
                    else if (root.TryGetProperty("errorDetails", out ed))
                        message = ed.GetRawText();

                    if (message?.Length > 500) message = message[..500];

                    return new NicEwbResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "NIC_ERROR",
                        ErrorMessage = message
                    };
                }

                // 6. Decrypt response data — EWB API uses "data" (lowercase)
                var encResult = root.TryGetProperty("data", out var rdp) ? rdp.GetString()
                    : root.TryGetProperty("Data", out rdp) ? rdp.GetString() : null;

                if (string.IsNullOrEmpty(encResult))
                    return new NicEwbResultDto
                    {
                        IsSuccess = false,
                        ErrorCode = "NIC_ERROR",
                        ErrorMessage = "NIC success response had no data field."
                    };

                var decResult = Encoding.UTF8.GetString(AesDecryptEcb(encResult, sek));
                using var resultDoc = JsonDocument.Parse(decResult);
                var resultRoot = resultDoc.RootElement;

                long? ewbNo = null;
                string? ewbDate = null;
                string? ewbValidTill = null;

                if (resultRoot.TryGetProperty("ewayBillNo", out var en) || resultRoot.TryGetProperty("EwayBillNo", out en))
                    ewbNo = en.ValueKind == JsonValueKind.Number ? en.GetInt64()
                        : long.TryParse(en.GetString(), out var v) ? v : null;

                if (resultRoot.TryGetProperty("ewayBillDate", out var ed2) || resultRoot.TryGetProperty("EwayBillDate", out ed2))
                    ewbDate = ed2.GetString();

                if (resultRoot.TryGetProperty("validUpto", out var vu) || resultRoot.TryGetProperty("ValidUpto", out vu))
                    ewbValidTill = vu.GetString();

                return new NicEwbResultDto
                {
                    IsSuccess = true,
                    EwbNo = ewbNo,
                    EwbDate = ewbDate,
                    EwbValidTill = ewbValidTill
                };
            }
            catch (Exception ex)
            {
                var source = ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim() ?? "unknown";
                return new NicEwbResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "SERVICE_ERROR",
                    ErrorMessage = $"{ex.Message} | Source: {source}"
                };
            }
        }

        // Loads UnitAddress + City for FromUnitId / ToUnitId and fills any
        // address fields the caller left blank. Same SQL pattern used by
        // LoadNicDataAsync for IRN-based EInvoices.
        private async Task EnrichStandalonePayloadAddressesAsync(
            StandaloneEwbPayloadDto p, CancellationToken ct)
        {
            const string addrSql = @"
                SELECT TOP 1 AddressLine1, AddressLine2, CityId, PinCode
                FROM AppData.UnitAddress
                WHERE UnitId = @unitId
                ORDER BY Id";

            if (p.FromUnitId is int fromUnit && fromUnit > 0
                && (string.IsNullOrWhiteSpace(p.FromAddr1) || p.FromPincode == 0 || string.IsNullOrWhiteSpace(p.FromPlace)))
            {
                var row = await _dbConnection.QueryFirstOrDefaultAsync<UnitAddressLite>(
                    addrSql, new { unitId = fromUnit });
                if (row != null)
                {
                    if (string.IsNullOrWhiteSpace(p.FromAddr1)) p.FromAddr1 = row.AddressLine1;
                    if (string.IsNullOrWhiteSpace(p.FromAddr2)) p.FromAddr2 = row.AddressLine2;
                    if (p.FromPincode == 0) p.FromPincode = row.PinCode;
                    if (string.IsNullOrWhiteSpace(p.FromPlace) && row.CityId > 0)
                    {
                        var city = await _cityLookup.GetByIdAsync(row.CityId);
                        if (!string.IsNullOrWhiteSpace(city?.CityName)) p.FromPlace = city!.CityName!;
                    }
                }
            }

            if (p.ToUnitId is int toUnit && toUnit > 0
                && (string.IsNullOrWhiteSpace(p.ToAddr1) || p.ToPincode == 0 || string.IsNullOrWhiteSpace(p.ToPlace)))
            {
                var row = await _dbConnection.QueryFirstOrDefaultAsync<UnitAddressLite>(
                    addrSql, new { unitId = toUnit });
                if (row != null)
                {
                    if (string.IsNullOrWhiteSpace(p.ToAddr1)) p.ToAddr1 = row.AddressLine1;
                    if (string.IsNullOrWhiteSpace(p.ToAddr2)) p.ToAddr2 = row.AddressLine2;
                    if (p.ToPincode == 0) p.ToPincode = row.PinCode;
                    if (string.IsNullOrWhiteSpace(p.ToPlace) && row.CityId > 0)
                    {
                        var city = await _cityLookup.GetByIdAsync(row.CityId);
                        if (!string.IsNullOrWhiteSpace(city?.CityName)) p.ToPlace = city!.CityName!;
                    }
                }
            }
        }

        private sealed class UnitAddressLite
        {
            public string? AddressLine1 { get; set; }
            public string? AddressLine2 { get; set; }
            public int CityId { get; set; }
            public int PinCode { get; set; }
        }

        // Local pre-flight checks for the standalone EWB payload — same spirit as
        // CollectValidationErrors() in the DC handler but kept here so the service
        // is also defensible when called from other modules later.
        private static List<string> ValidateStandalonePayload(StandaloneEwbPayloadDto p)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(p.DocNo))         errors.Add("DocNo is required.");
            if (string.IsNullOrWhiteSpace(p.DocDate))       errors.Add("DocDate is required (dd/MM/yyyy).");
            if (string.IsNullOrWhiteSpace(p.FromGstin))     errors.Add("FromGstin is required.");
            if (string.IsNullOrWhiteSpace(p.FromTrdName))   errors.Add("FromTrdName is required.");
            if (string.IsNullOrWhiteSpace(p.ToGstin))       errors.Add("ToGstin is required.");
            if (string.IsNullOrWhiteSpace(p.ToTrdName))     errors.Add("ToTrdName is required.");
            if (p.FromPincode <= 0)                         errors.Add("FromPincode is required.");
            if (p.ToPincode <= 0)                           errors.Add("ToPincode is required.");
            if (p.FromStateCode <= 0)                       errors.Add("FromStateCode is required.");
            if (p.ToStateCode <= 0)                         errors.Add("ToStateCode is required.");
            if (p.TransDistance <= 0 || p.TransDistance > 4000)
                errors.Add("TransDistance must be between 1 and 4000 km.");
            if (p.ItemList == null || p.ItemList.Count == 0)
                errors.Add("ItemList must contain at least one item.");
            else
            {
                foreach (var item in p.ItemList)
                {
                    if (string.IsNullOrWhiteSpace(item.ProductName))
                        errors.Add($"ProductName missing on item {item.ItemNo}.");
                    if (item.HsnCode <= 0)
                        errors.Add($"HsnCode missing on item {item.ItemNo}.");
                    if (item.Quantity <= 0)
                        errors.Add($"Quantity must be > 0 on item {item.ItemNo}.");
                }
            }

            return errors;
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

            // ── NIC Error 2240: GST rate must be a notified rate ──────────────
            // First tries stored GstPercentage; if not notified, computes from tax amounts.
            // Only flags error when NEITHER approach yields a valid rate.
            foreach (var detail in data.Details)
            {
                var resolved = ResolveGstRate(detail.GstPercentage, detail.TaxableAmount,
                    detail.CGST, detail.SGST, detail.IGST);
                if (!IsNotifiedRate(resolved))
                {
                    var effectiveInfo = detail.TaxableAmount > 0
                        ? $" Computed rate from tax amounts: {Math.Round((detail.CGST + detail.SGST + detail.IGST) / detail.TaxableAmount * 100m, 2)}%."
                        : string.Empty;
                    errors.Add($"Item '{detail.ItemName}' (Line {detail.ItemSno}): GST rate {detail.GstPercentage}% " +
                               $"is not a NIC-notified rate.{effectiveInfo} " +
                               $"Valid rates: 0, 0.1, 0.25, 1, 1.5, 3, 5, 7.5, 12, 18, 28. " +
                               $"Please correct the GST percentage in the Sales Invoice.");
                }
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

            // NIC auth flow:
            // 1. Generate random 32-byte AES key (AppKey) for this session
            // 2. RSA-encrypt ONLY the Password and AppKey fields individually
            // 3. Send JSON with plain UserName + encrypted Password + encrypted AppKey
            // 4. NIC decrypts Password & AppKey with their private key, authenticates
            // 5. NIC encrypts SEK with our AppKey (AES-256 ECB) and returns it
            var appKeyBytes = new byte[32];
            RandomNumberGenerator.Fill(appKeyBytes);
            var appKeyBase64 = Convert.ToBase64String(appKeyBytes);

            // NIC C# sample flow:
            // 1. Serialize JSON with plain Password & AppKey
            // 2. UTF-8 encode → bytes
            // 3. Base64 encode bytes → string
            // 4. RSA encrypt the Base64 string with NIC public key
            // 5. Wrap in {"Data": "<rsa_encrypted_base64>"}
            // Build auth JSON exactly as NIC portal does
            var authJson = $"{{\"UserName\":\"{cfg.UserName}\",\"Password\":\"{cfg.Password}\",\"AppKey\":\"{appKeyBase64}\",\"ForceRefreshAccessToken\":false}}";

            var authBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authJson));
            var encryptedData = RsaEncryptWithPublicKey(authBase64);

            var requestBody = JsonSerializer.Serialize(new { Data = encryptedData }, _jsonOptions);

            using var req = new HttpRequestMessage(HttpMethod.Post, cfg.AuthPath)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };
            req.Headers.Add("client-id", cfg.ClientId);
            req.Headers.Add("client-secret", cfg.ClientSecret);
            req.Headers.Add("gstin", cfg.GstinForAuth);

            using var resp = await client.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            // NIC may return HTML error pages — detect and report
            if (string.IsNullOrWhiteSpace(body) || body.TrimStart().StartsWith('<'))
            {
                throw new InvalidOperationException(
                    $"NIC auth endpoint returned non-JSON response (HTTP {(int)resp.StatusCode}). " +
                    $"URL: {cfg.AuthPath}. Response: {body[..Math.Min(body.Length, 500)]}");
            }

            // Log full response for debugging
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(body);
            }
            catch
            {
                throw new InvalidOperationException($"NIC auth returned invalid JSON. Response: {body[..Math.Min(body.Length, 500)]}");
            }

            var root = doc.RootElement;

            // Parse Status — could be int (1) or string ("1")
            int statusVal = 0;
            if (root.TryGetProperty("Status", out var statusEl))
                statusVal = statusEl.ValueKind == JsonValueKind.Number ? statusEl.GetInt32() : int.Parse(statusEl.GetString() ?? "0");
            else if (root.TryGetProperty("status", out var statusEl2))
                statusVal = statusEl2.ValueKind == JsonValueKind.Number ? statusEl2.GetInt32() : int.Parse(statusEl2.GetString() ?? "0");
            else
                throw new InvalidOperationException($"NIC auth response missing 'Status'. Response: {body[..Math.Min(body.Length, 500)]}");

            if (statusVal != 1)
            {
                var msg = root.TryGetProperty("ErrorDetails", out var ed) ? ed.ToString()
                    : root.TryGetProperty("error", out var er) ? er.ToString()
                    : root.TryGetProperty("message", out var m) ? m.GetString()
                    : $"Auth failed. Full response: {body[..Math.Min(body.Length, 500)]}";
                throw new InvalidOperationException($"NIC auth failed: {msg}");
            }

            // Data could be a JSON object or an encrypted string
            if (!root.TryGetProperty("Data", out var dataEl) && !root.TryGetProperty("data", out dataEl))
                throw new InvalidOperationException($"NIC auth response missing 'Data'. Response: {body[..Math.Min(body.Length, 500)]}");

            // If Data is a string, it's AES-encrypted with our AppKey — decrypt it
            JsonElement dataObj;
            if (dataEl.ValueKind == JsonValueKind.String)
            {
                var decryptedDataJson = Encoding.UTF8.GetString(AesDecryptEcb(dataEl.GetString()!, appKeyBytes));
                dataObj = JsonDocument.Parse(decryptedDataJson).RootElement;
            }
            else
            {
                dataObj = dataEl;
            }

            var authToken = (dataObj.TryGetProperty("AuthToken", out var at) ? at.GetString()
                : dataObj.TryGetProperty("authToken", out var at2) ? at2.GetString() : null)
                ?? throw new InvalidOperationException($"AuthToken missing. Data: {dataObj}");

            var sekBase64 = (dataObj.TryGetProperty("Sek", out var sk) ? sk.GetString()
                : dataObj.TryGetProperty("sek", out var sk2) ? sk2.GetString() : null)
                ?? throw new InvalidOperationException($"Sek missing. Data: {dataObj}");

            // Decrypt Sek using AppKey (already decoded above)
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
                SELECT h.Id, h.UnitId, h.PartyId, h.InvoiceNo,
                       FORMAT(h.InvoiceDate, 'dd/MM/yyyy') AS InvoiceDateFormatted,
                       h.DocType, h.SupplyType, h.PlaceOfSupply, h.GstNo,
                       h.ReverseCharge, h.CGST, h.SGST, h.IGST, h.Cess,
                       h.StateCess, 0 AS Discount, h.OtherCharges, h.RoundOff,
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
                    "SELECT CompanyName, GstNumber FROM AppData.Company WHERE Id = @companyId";
                companyRow = await _dbConnection.QueryFirstOrDefaultAsync<CompanyRow>(
                    companySql, new { companyId = unitRow.CompanyId });
            }

            // ── 2e. Unit address (AppData schema) ────────────────────────────
            const string unitAddrSql = @"
                SELECT TOP 1 AddressLine1, AddressLine2, CityId, PinCode, ContactNumber
                FROM AppData.UnitAddress
                WHERE UnitId = @unitId
                ORDER BY Id";
            var unitAddr = await _dbConnection.QueryFirstOrDefaultAsync<UnitAddressRow>(
                unitAddrSql, new { unitId = header.UnitId });

            // ── 2f. Buyer legal name (via lookup) ─────────────────────────────
            var partyDto = await _partyLookup.GetByIdAsync(header.PartyId);

            // ── 2g. Buyer address (PartyManagement schema) ────────────────────
            const string partyAddrSql = @"
                SELECT TOP 1 AddressLine1, AddressLine2, CityId, PostalCode
                FROM Party.PartyAddress
                WHERE PartyId = @partyId
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
                InvoiceDateFormatted = header.InvoiceDateFormatted,
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
                SellerLocation = unitAddr != null ? (await _cityLookup.GetByIdAsync(unitAddr.CityId, ct))?.CityName : null,
                SellerPinCode = unitAddr?.PinCode.ToString(),
                SellerPhone = unitAddr?.ContactNumber,
                SellerEmail = null,

                BuyerLegalName = partyDto?.PartyName,
                BuyerTradeName = partyDto?.PartyName,
                BuyerAddr1 = partyAddr?.AddressLine1,
                BuyerAddr2 = partyAddr?.AddressLine2,
                BuyerLocation = partyAddr != null ? (await _cityLookup.GetByIdAsync(partyAddr.CityId, ct))?.CityName : null,
                BuyerPinCode = partyAddr?.PostalCode,
                BuyerPhone = null,
                BuyerEmail = null,

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

            // NIC requires: amounts max 2 decimals, qty/rate max 3 decimals
            static decimal R2(decimal v) => Math.Round(v, 2);
            static decimal R3(decimal v) => Math.Round(v, 3);

            var itemList = d.Details.Select(item => new
            {
                SlNo = item.ItemSno.ToString(),
                PrdDesc = item.ItemName,
                IsServc = item.IsService,
                HsnCd = item.HsnNo,
                Qty = R3(item.Qty),
                FreeQty = 0m,
                Unit = ResolveUqcCode(item.UOM),
                UnitPrice = R3(item.UnitPrice),
                TotAmt = R2(item.GrossAmount),
                Discount = R2(item.Discount),
                PreTaxVal = 0m,
                AssAmt = R2(item.TaxableAmount),
                GstRt = ResolveGstRate(item.GstPercentage, item.TaxableAmount,
                    item.CGST, item.SGST, item.IGST),
                IgstAmt = R2(item.IGST),
                CgstAmt = R2(item.CGST),
                SgstAmt = R2(item.SGST),
                CesRt = R2(item.CessRate),
                CesAmt = R2(item.CessAmount),
                CesNonAdvlAmt = 0m,
                StateCesRt = 0m,
                StateCesAmt = 0m,
                StateCesNonAdvlAmt = 0m,
                OthChrg = 0m,
                TotItemVal = R2(item.TotalAmount)
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
                    Dt = d.InvoiceDateFormatted ?? "01/01/2026"
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
                    AssVal = R2(d.Details.Sum(x => x.TaxableAmount)),
                    IgstVal = R2(d.IGST),
                    CgstVal = R2(d.CGST),
                    SgstVal = R2(d.SGST),
                    CesVal = R2(d.Cess),
                    StCesVal = R2(d.StateCess),
                    Discount = R2(d.Discount),
                    OthChrg = R2(d.OtherCharges),
                    RndOffAmt = R2(d.RoundOff),
                    TotInvVal = R2(d.InvoiceAmount)
                }
            };

            // Case 1: Include EwbDtls when transport details are provided
            // NIC generates both IRN + e-Waybill in a single call
            if (ewb is not null && ewb.Distance > 0)
            {
                // NIC TransMode must be "1"/"2"/"3"/"4". Use DB value if valid; default to "1" (Road)
                // when VehicleNo or TransDocNo is present (NIC error 4028 if omitted in that case).
                var ewbTransMode = ewb.TransMode is "1" or "2" or "3" or "4" ? ewb.TransMode : null;
                if (ewbTransMode == null && (!string.IsNullOrWhiteSpace(ewb.VehNo) || !string.IsNullOrWhiteSpace(ewb.TransDocNo)))
                    ewbTransMode = "1";

                payload["EwbDtls"] = new
                {
                    TransId = ewb.TransId,
                    TransName = ewb.TransName,
                    Distance = ewb.Distance,
                    TransDocNo = ewb.TransDocNo,
                    TransDocDt = ewb.TransDocDt,
                    VehNo = ewb.VehNo,
                    VehType = ewb.VehType ?? "R",
                    TransMode = ewbTransMode
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

            var body = JsonSerializer.Serialize(new { Data = encryptedPayload }, _jsonOptions);

            using var req = new HttpRequestMessage(HttpMethod.Post, cfg.GenerateIrnPath)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            req.Headers.Add("client-id", cfg.ClientId);
            req.Headers.Add("client-secret", cfg.ClientSecret);
            req.Headers.Add("user_name", cfg.UserName);
            req.Headers.Add("authtoken", authToken);
            req.Headers.Add("gstin", cfg.GstinForAuth);

            using var resp = await client.SendAsync(req, ct);
            var responseBody = await resp.Content.ReadAsStringAsync(ct);

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Parse status — handle both "Status"/"status" and int/string
            int irnStatusVal = 0;
            if (root.TryGetProperty("Status", out var irnSt))
                irnStatusVal = irnSt.ValueKind == JsonValueKind.Number ? irnSt.GetInt32() : int.TryParse(irnSt.GetString(), out var sv) ? sv : 0;
            else if (root.TryGetProperty("status", out var irnSt2))
                irnStatusVal = irnSt2.ValueKind == JsonValueKind.Number ? irnSt2.GetInt32() : int.TryParse(irnSt2.GetString(), out var sv2) ? sv2 : 0;

            if (irnStatusVal != 1)
            {
                // Extract error details from NIC error response
                var message = root.TryGetProperty("ErrorDetails", out var edArr) ? edArr.ToString()
                    : root.TryGetProperty("errorDetails", out var errArr) ? errArr.ToString()
                    : root.TryGetProperty("message", out var msg) ? msg.GetString()
                    : root.TryGetProperty("Message", out var msg2) ? msg2.GetString()
                    : $"Unknown error. Response: {responseBody[..Math.Min(responseBody.Length, 500)]}";

                return new NicIrnResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "NIC_ERROR",
                    ErrorMessage = message
                };
            }

            // Decrypt the response data using Sek
            var encData = root.TryGetProperty("Data", out var dataP) ? dataP
                : root.TryGetProperty("data", out var dataP2) ? dataP2
                : throw new InvalidOperationException($"NIC IRN response missing 'Data'. Response: {responseBody[..Math.Min(responseBody.Length, 500)]}");
            var encryptedResponse = encData.GetString()
                ?? throw new InvalidOperationException("NIC response 'Data' is null.");

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

            var body = JsonSerializer.Serialize(new { Data = encryptedPayload }, _jsonOptions);

            var ewbPath = cfg.BaseUrl +
                (_configuration.GetSection("NicEInvoice")["GenerateEwbPath"]
                    ?? "/eiewb/v1.03/ewaybill");

            using var req = new HttpRequestMessage(HttpMethod.Post, ewbPath)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            req.Headers.Add("client-id", cfg.ClientId);
            req.Headers.Add("client-secret", cfg.ClientSecret);
            req.Headers.Add("user_name", cfg.UserName);
            req.Headers.Add("authtoken", authToken);
            req.Headers.Add("gstin", cfg.GstinForAuth);

            using var resp = await client.SendAsync(req, ct);
            var responseBody = await resp.Content.ReadAsStringAsync(ct);

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // NIC returns Status/status — check both
            var statusOk = false;
            if (root.TryGetProperty("Status", out var statusProp) || root.TryGetProperty("status", out statusProp))
            {
                var statusVal = statusProp.ValueKind == JsonValueKind.Number
                    ? statusProp.GetInt32()
                    : int.TryParse(statusProp.GetString(), out var sv) ? sv : 0;
                statusOk = statusVal == 1;
            }

            if (!statusOk)
            {
                // Try multiple NIC error field names
                var message = root.TryGetProperty("ErrorDetails", out var ed) ? ed.GetRawText()
                    : root.TryGetProperty("errorDetails", out ed) ? ed.GetRawText()
                    : root.TryGetProperty("error", out ed) ? ed.GetRawText()
                    : responseBody.Length > 500 ? responseBody[..500] : responseBody;

                return new NicEwbResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "NIC_ERROR",
                    ErrorMessage = message
                };
            }

            // Decrypt the response data using Sek
            var encData = root.TryGetProperty("Data", out var dataProp) ? dataProp.GetString()
                : root.TryGetProperty("data", out dataProp) ? dataProp.GetString()
                : null;

            if (string.IsNullOrEmpty(encData))
            {
                return new NicEwbResultDto
                {
                    IsSuccess = false,
                    ErrorCode = "NIC_ERROR",
                    ErrorMessage = "NIC e-Waybill response missing 'Data' field. Raw: " +
                        (responseBody.Length > 300 ? responseBody[..300] : responseBody)
                };
            }

            var decryptedJson = Encoding.UTF8.GetString(AesDecryptEcb(encData, sek));
            using var ewbDoc = JsonDocument.Parse(decryptedJson);
            var ewbRoot = ewbDoc.RootElement;

            return new NicEwbResultDto
            {
                IsSuccess = true,
                EwbNo = ewbRoot.TryGetProperty("EwbNo", out var ewbNo)
                    && ewbNo.ValueKind == JsonValueKind.Number ? ewbNo.GetInt64() : null,
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

        // NIC sandbox public key (einv_sandbox.PEM) for RSA encryption of auth payload
        // Downloaded from: https://einv-apisandbox.nic.in/einvapiclient/EncDesc/GetPublicKey.aspx
        // Section: "Public Key for Encryption of Password and App Key for Authentication"
        private const string NicSandboxPublicKeyBase64 =
            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArxd93uLDs8HTPqcSPpxZ" +
            "rf0Dc29r3iPp0a8filjAyeX4RAH6lWm9qFt26CcE8ESYtmo1sVtswvs7VH4Bjg/F" +
            "DlRpd+MnAlXuxChij8/vjyAwE71ucMrmZhxM8rOSfPML8fniZ8trr3I4R2o4xWh6" +
            "no/xTUtZ02/yUEXbphw3DEuefzHEQnEF+quGji9pvGnPO6Krmnri9H4WPY0ysPQQ" +
            "Qd82bUZCk9XdhSZcW/am8wBulYokITRMVHlbRXqu1pOFmQMO5oSpyZU3pXbsx+Ox" +
            "IOc4EDX0WMa9aH4+snt18WAXVGwF2B4fmBk7AtmkFzrTmbpmyVqA3KO2IjzMZPw0" +
            "hQIDAQAB";

        private static string RsaEncryptWithPublicKey(string plainText)
        {
            // Match NIC C# sample exactly: RSACryptoServiceProvider + ImportParameters
            var publicKeyBytes = Convert.FromBase64String(NicSandboxPublicKeyBase64);

            // Parse DER-encoded SubjectPublicKeyInfo to get RSA parameters
            using var tempRsa = RSA.Create();
            tempRsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            var rsaParams = tempRsa.ExportParameters(false);

            // Use RSACryptoServiceProvider as NIC sample does
            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);

            var dataToEncrypt = Encoding.UTF8.GetBytes(plainText);
            var encrypted = rsa.Encrypt(dataToEncrypt, false); // false = PKCS1 v1.5
            return Convert.ToBase64String(encrypted);
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
            public string? InvoiceDateFormatted { get; set; }
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
        private sealed class UnitAddressRow { public string? AddressLine1 { get; set; } public string? AddressLine2 { get; set; } public int CityId { get; set; } public int PinCode { get; set; } public string? ContactNumber { get; set; } }
        private sealed class PartyAddressRow { public string? AddressLine1 { get; set; } public string? AddressLine2 { get; set; } public int CityId { get; set; } public string? PostalCode { get; set; } }

        private sealed class NicEInvoiceData
        {
            public int Id { get; set; }
            public int UnitId { get; set; }
            public int PartyId { get; set; }
            public string? InvoiceNo { get; set; }
            public string? InvoiceDateFormatted { get; set; }
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
