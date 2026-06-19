namespace PurchaseManagement.Application.Arrival.Dto
{
    /// <summary>
    /// Everything the arrival form prefills when a Raw Material PO is selected: balance qty per item,
    /// plus the approved freight (transporter/party + rate) from the Approved Freight RFQ linked to the PO.
    /// Supersedes the balance-qty call. ApprovedFreight is null when the PO has no Approved Freight RFQ.
    /// </summary>
    public sealed class ArrivalPoPrefillDto
    {
        public IReadOnlyList<ArrivalBalanceQtyDto> Items { get; set; } = new List<ArrivalBalanceQtyDto>();
        public ApprovedFreightDto? ApprovedFreight { get; set; }
    }
}
