namespace SalesManagement.Application.DocumentSequence.Dto
{
    public class DocumentSequenceGeneratedDto
    {
        public int Id { get; set; }
        public int TransactionTypeId { get; set; }
        public int FinancialYearId { get; set; }
        public int DocNo { get; set; }
        public int UnitId { get; set; }
        public string? UnitShortName { get; set; }
        public string? TypeShortName { get; set; }
        public string? FinancialYearName { get; set; }
        public string? GeneratedDocumentNumber { get; set; }
    }
}
