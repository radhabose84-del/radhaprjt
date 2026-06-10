using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    /// <summary>
    /// Mix Code master — cotton/fibre mix codes used during bale inward (Arrival).
    /// Backs <c>Purchase.ArrivalDetail.MixCodeId</c>.
    /// Table: [Purchase].[MixCodeMaster].
    /// </summary>
    public class MixCodeMaster : BaseEntity
    {
        public string MixCode { get; set; } = default!;
        public string MixCodeDesc { get; set; } = default!;
    }
}
