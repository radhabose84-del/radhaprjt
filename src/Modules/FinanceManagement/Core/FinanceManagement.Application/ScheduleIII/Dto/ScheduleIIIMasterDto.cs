namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ScheduleIIIMasterDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }              // cross-module lookup (ICompanyLookup)
        public int DivisionId { get; set; }
        public string? DivisionName { get; set; }             // cross-module lookup (IDivisionLookup)
        public int StatusId { get; set; }
        public string? StructureStatusName { get; set; }      // MiscMaster JOIN
        public bool TextileSplitEnabled { get; set; }
        public bool IsActive { get; set; }

        public List<ScheduleIIISectionDto> Sections { get; set; } = new();
        public List<ScheduleIIISubTotalDto> SubTotals { get; set; } = new();
    }
}
