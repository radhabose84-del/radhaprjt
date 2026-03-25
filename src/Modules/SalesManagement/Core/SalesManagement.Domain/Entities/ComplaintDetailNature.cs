namespace SalesManagement.Domain.Entities
{
    public class ComplaintDetailNature
    {
        public int Id { get; set; }
        public int ComplaintDetailId { get; set; }

        // Same-module FK → Sales.MiscMaster (NatureOfComplaint)
        public int NatureOfComplaintId { get; set; }
        public MiscMaster? NatureOfComplaintMisc { get; set; }

        // Parent navigation
        public ComplaintDetail? ComplaintDetail { get; set; }
    }
}
