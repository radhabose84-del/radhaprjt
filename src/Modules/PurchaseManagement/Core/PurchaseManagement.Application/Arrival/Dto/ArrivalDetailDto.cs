namespace PurchaseManagement.Application.Arrival.Dto
{
    public class ArrivalDetailDto
    {
        public int Id { get; set; }
        public int ArrivalHeaderId { get; set; }

        public int ItemId { get; set; }
        public string? ItemName { get; set; }

        public int HsnId { get; set; }
        public string? HsnCode { get; set; }

        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }

        public int MixCodeId { get; set; }   // lookup/master TBD — name not populated yet

        public int UomId { get; set; }
        public string? UomName { get; set; }

        public decimal Rate { get; set; }

        public decimal OrderedQty { get; set; }
        public decimal ArrivedQty { get; set; }
        public decimal CancelledQty { get; set; }
        public decimal BalanceQty { get; set; }

        public string? BatchNumber { get; set; }
        public long BaleNumberFrom { get; set; }
        public long BaleNumberTo { get; set; }
        public int TotalBaleCount { get; set; }

        /// <summary>
        /// Capture mode (derived from the stock ledger). Consolidated lines return an empty Bales
        /// list; Individual lines return one Bales entry per captured bale.
        /// </summary>
        public bool IsIndividual { get; set; }
        public List<ArrivalBaleRowDto> Bales { get; set; } = new();
    }

    public class ArrivalBaleRowDto
    {
        public long BaleNo { get; set; }
        public long? BarcodeNumber { get; set; }
        public decimal BaleWeight { get; set; }
        public int? BaleCaptureMethodId { get; set; }
    }
}
