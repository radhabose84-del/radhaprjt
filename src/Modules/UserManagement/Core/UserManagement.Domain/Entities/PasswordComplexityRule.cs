using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class PasswordComplexityRule :BaseEntity
    {

       public int Id { get; set; }
       
        public string? PwdComplexityRule  { get; set; }

       
    }
}