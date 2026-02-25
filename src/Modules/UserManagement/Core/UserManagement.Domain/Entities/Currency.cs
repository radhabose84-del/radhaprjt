using UserManagement.Domain.Common;


namespace UserManagement.Domain.Entities
{
    public class Currency : BaseEntity
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public CompanySettings? CompanySettings { get; set; }
    
    }
}