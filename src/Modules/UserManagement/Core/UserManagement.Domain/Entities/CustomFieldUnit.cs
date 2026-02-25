namespace UserManagement.Domain.Entities
{
    public class CustomFieldUnit
    {
        public int Id { get; set; }
        public int CustomFieldId { get; set; }
        public CustomField? CustomField { get; set; }
        public int UnitId { get; set; }
        public Unit? Unit { get; set; }
    }
}