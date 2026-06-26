namespace FinanceManagement.Application.ScheduleIII.Dto
{
    // One parsed row from the Schedule III section/line-item import sheet. Rows with the same
    // SectionName form one section (StatementType + Nature must be consistent across the section's rows).
    public class ScheduleIIIImportRowInputDto
    {
        public int RowNo { get; set; }
        public string? SectionName { get; set; }
        public string? StatementType { get; set; }   // S3_STMT_TYPE code or description (e.g. PL / "Profit & Loss")
        public string? Nature { get; set; }           // S3_NATURE code or description (e.g. Income / Expense)
        public string? LineCode { get; set; }
        public string? LineName { get; set; }
        public string? NoteReference { get; set; }
        public bool IsSplitLine { get; set; }
    }

    public class ScheduleIIIImportErrorDto
    {
        public int RowNo { get; set; }
        public string? ColumnName { get; set; }
        public string? Message { get; set; }
    }

    // Result of an import run. All-or-nothing: on any error, Committed=false and nothing is created.
    public class ImportScheduleIIIResultDto
    {
        public int TotalRows { get; set; }
        public int SectionsCreated { get; set; }
        public int ItemsCreated { get; set; }
        public int ErrorRows { get; set; }
        public string? Status { get; set; }           // COMMITTED / FAILED
        public bool Committed { get; set; }
        public List<ScheduleIIIImportErrorDto> Errors { get; set; } = new();
        public List<int> CreatedSectionIds { get; set; } = new();
    }

    public class ScheduleIIIImportFileDto
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = "ScheduleIII_Import_Template.xlsx";
        public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }

    // MiscMaster option (S3_STMT_TYPE / S3_NATURE) used to resolve a sheet's text to a MiscMaster id.
    public class ScheduleIIIMiscOptionDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
}
