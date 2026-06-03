namespace FinanceManagement.Application.TransactionTypeMaster.Dto
{
    public class TransactionTypeMasterDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int ModuleId { get; set; }
        public string? ModuleName { get; set; }
        public int MenuId { get; set; }
        public string? MenuName { get; set; }
        public string? TypeName { get; set; }
        public string? ShortName { get; set; }
        public string? Description { get; set; }
        public bool IsGate { get; set; }
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
