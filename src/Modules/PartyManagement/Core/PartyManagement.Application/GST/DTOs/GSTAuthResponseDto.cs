using Newtonsoft.Json;

namespace PartyManagement.Application.GST.DTOs
{
    public class GSTAuthResponseDto
    {
        [JsonProperty("Status")]
        public int Status { get; set; }

        [JsonProperty("Data")]
        public GSTAuthDataDto? Data { get; set; }
    }

    public class GSTAuthDataDto
    {
          [JsonProperty("ClientId")]
    public string? ClientId { get; set; }

    [JsonProperty("UserName")]
    public string? UserName { get; set; }

    [JsonProperty("AuthToken")]
    public string? AuthToken { get; set; }

    [JsonProperty("Sek")]
    public string? Sek { get; set; }

    [JsonProperty("TokenExpiry")]
    public DateTimeOffset TokenExpiry { get; set; }
    }
    public class GSTINDetailsDto
    {
        [JsonProperty("gstin")]
        public string? Gstin { get; set; }

        [JsonProperty("tradeName")]
        public string? TradeName { get; set; }

        [JsonProperty("legalName")]
        public string? LegalName { get; set; }

        [JsonProperty("stateCode")]
        public string? StateCode { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("addrPncd")]
        public string? AddrPncd { get; set; }

        [JsonProperty("txpType")]
        public string? TxpType { get; set; }

        [JsonProperty("addrBno")]
        public string? AddrBno { get; set; }

        [JsonProperty("addrSt")]
        public string? AddrSt { get; set; }

        [JsonProperty("addrLoc")]
        public string? AddrLoc { get; set; }

        [JsonProperty("addrFlno")]
        public string? AddrFlno { get; set; }
    }
}
