namespace GateEntryManagement.Application.GateInward.Dto
{
    public class GateInwardDtlDto
    {
        public int Id { get; set; }
        public int GateInwardHdrId { get; set; }
        public int ReferenceDocTypeId { get; set; }
        public string? ReferenceDocNo { get; set; }
        public string? PartyName { get; set; }

        // Minimum PO context kept on the Gate row; for display join with PO master at read time.
        public int? PoId { get; set; }
        public int? PoSlNoLocal { get; set; }
        public decimal? DcQuantity { get; set; }
    }
}
