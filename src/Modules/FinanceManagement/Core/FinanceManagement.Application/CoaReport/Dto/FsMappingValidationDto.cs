namespace FinanceManagement.Application.CoaReport.Dto
{
    // US-GL02-15 (AC4) — FS-mapping (Schedule III) validation summary. Go-live is clean when
    // UnmappedCount = 0; otherwise Unmapped lists the active leaf groups still without a line.
    public class FsMappingValidationDto
    {
        public int TotalLeafGroups { get; set; }
        public int MappedCount { get; set; }
        public int UnmappedCount { get; set; }
        public bool IsClean => UnmappedCount == 0;

        public List<FsMappingUnmappedItemDto> Unmapped { get; set; } = new();
    }

    public class FsMappingUnmappedItemDto
    {
        public int AccountGroupId { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public int Level { get; set; }
        public int AccountCount { get; set; }            // GL accounts attached to this unmapped group
    }
}
