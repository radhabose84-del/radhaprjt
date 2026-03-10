namespace PartyManagement.Domain.Entities
{
    public class SalesType
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public PartyMaster Party { get; set; } = null!;
        public int? SalesSegmentId { get; set; }
        public int? OrderTypeId { get; set; }
        public int? IncotermId { get; set; }
        public int? PaymentTermsId { get; set; }
        public int? ShippingConditionId { get; set; }
        public MiscMaster? ShippingConditionMisc { get; set; }
        public int? AccountAssignmentId { get; set; }
        public MiscMaster? AccountAssignmentMisc { get; set; }
        public byte Active { get; set; }
    }
}
