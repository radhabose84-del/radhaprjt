namespace GateEntryManagement.Application.GateInward.Dto
{
    public class GateInwardDtlDto
    {
        public int Id { get; set; }
        public int GateInwardHdrId { get; set; }
        public int ReferenceDocTypeId { get; set; }
        public string? ReferenceDocNo { get; set; }
        public string? PartyName { get; set; }
    }
}
