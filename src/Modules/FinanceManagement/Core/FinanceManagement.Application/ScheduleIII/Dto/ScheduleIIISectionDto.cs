namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ScheduleIIISectionDto
    {
        public int Id { get; set; }
        public int StructureId { get; set; }
        public string? SectionName { get; set; }
        public int StatementTypeId { get; set; }
        public string? StatementTypeName { get; set; }        // MiscMaster JOIN
        public int NatureId { get; set; }
        public string? NatureName { get; set; }               // MiscMaster JOIN
        public int DisplayOrder { get; set; }

        public List<ScheduleIIILineItemDto> LineItems { get; set; } = new();
    }
}
