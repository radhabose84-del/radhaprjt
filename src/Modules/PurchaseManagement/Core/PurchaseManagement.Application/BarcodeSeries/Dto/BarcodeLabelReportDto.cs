namespace PurchaseManagement.Application.BarcodeSeries.Dto
{
    /// <summary>
    /// Print-ready payload for the bale-barcode labels of one barcode series: a letterhead block
    /// (resolved from the logged-in company/division/unit) plus the range expanded into individual
    /// barcode rows with their QR payload. Agent/Passed-By are supplied by the FE at print time
    /// (the numbering pool has no cotton-lot context).
    /// </summary>
    public sealed class BarcodeLabelReportDto
    {
        public BarcodeLetterheadDto Letterhead { get; set; } = new();

        public string? SeriesNumber { get; set; }
        public string? Prefix { get; set; }

        /// <summary>Default agent caption printed on every label; the FE may override.</summary>
        public string AgentDefault { get; set; } = "DIRECT";

        /// <summary>Full number of barcodes in the series range (BarcodeEndNumber - BarcodeStartNumber + 1).</summary>
        public long TotalCount { get; set; }

        /// <summary>True when the range exceeded the per-request cap and <see cref="Labels"/> was truncated.</summary>
        public bool Truncated { get; set; }

        public List<BarcodeLabelItemDto> Labels { get; set; } = new();
    }

    public sealed class BarcodeLetterheadDto
    {
        public string? CompanyName { get; set; }
        public string? DivisionName { get; set; }
        public string? Address { get; set; }
    }

    public sealed class BarcodeLabelItemDto
    {
        /// <summary>Full barcode string, e.g. "BLC100002211" (prefix + number).</summary>
        public string Barcode { get; set; } = default!;

        /// <summary>Content to encode in the QR — the plain barcode string.</summary>
        public string QrPayload { get; set; } = default!;
    }
}
