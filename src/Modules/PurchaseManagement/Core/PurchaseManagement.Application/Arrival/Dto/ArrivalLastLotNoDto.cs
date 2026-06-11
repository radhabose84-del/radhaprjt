namespace PurchaseManagement.Application.Arrival.Dto
{
    /// <summary>
    /// Last lot number for the current unit. LotNo = the most recent ArrivalHeader Id
    /// (lot reference) for the unit; ArrivalNumber is its document number.
    /// </summary>
    public sealed class ArrivalLastLotNoDto
    {
        public int LotNo { get; set; }
        public string? ArrivalNumber { get; set; }
    }
}
