namespace GateEntryManagement.Application.GatePass.Dto
{
    public class GatePassDtlDto
    {
        public int Id { get; set; }
        public int GatePassHdrId { get; set; }
        public int DocTypeId { get; set; }
        public string? DocTypeCode { get; set; }
        public string? DocTypeName { get; set; }
        public int DocId { get; set; }
        public string? DocNo { get; set; }
        public string? PartyName { get; set; }
        public string? PartyCode { get; set; }
        public DateOnly? DocDate { get; set; }
        public string? ItemDescription { get; set; }
        public string? Uom { get; set; }
        public decimal TotalQty { get; set; }
        public decimal? GrossKgs { get; set; }
        public decimal? NetKgs { get; set; }
        public decimal? WithLoadKgs { get; set; }
        public decimal? WithoutLoadKgs { get; set; }
        public decimal? TotalWeight { get; set; }
    }
}
