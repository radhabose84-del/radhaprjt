namespace FinanceManagement.Application.DocumentSequence.Dto
{
    public class DocumentSequenceDto
    {
        public int Id { get; set; }
        public int TransactionTypeId { get; set; }
        public string? TypeName { get; set; }
        public string? TypeShortName { get; set; }
        public int UnitId { get; set; }
        public string? UnitShortName { get; set; }
        public int FinancialYearId { get; set; }
        public string? FinancialYearName { get; set; }
        public int DocNo { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
