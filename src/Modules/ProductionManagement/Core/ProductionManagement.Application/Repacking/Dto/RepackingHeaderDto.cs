namespace ProductionManagement.Application.Repacking.Dto
{
    public class RepackingHeaderDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int ProductionYear { get; set; }
        public string? RepackingNo { get; set; }
        public DateOnly RepackingDate { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public decimal LooseConeKgs { get; set; }
        public int OldPackHeaderId { get; set; }
        public string? OldPackNo { get; set; }
        public int? LooseHandlingId { get; set; }
        public string? LooseHandlingName { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public List<RepackingDetailDto>? RepackingDetails { get; set; }
        public List<OldPackDetailDto>? OldDetails { get; set; }
    }
}
