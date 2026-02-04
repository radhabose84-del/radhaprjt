using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class Entity : BaseEntity
    {
    public int Id { get; set; }
    public string? EntityCode { get; set; }
    public string? EntityName { get; set; }
    public string? EntityDescription { get; set; }
    public string? Address { get; set; }
    public string? Phone  { get; set; }
    public string? Email  { get; set; }
    public AdminSecuritySettings? AdminSecuritySettings { get; set; }
    }
}