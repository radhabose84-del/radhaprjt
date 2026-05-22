using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Domain.Common
{
    public abstract class BaseEntity
    {
        public Status IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
      
        public string? CreatedByName { get; set; }
      
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
       
        public string? ModifiedByName { get; set; }
       
        public string? ModifiedIP { get; set; }
        public IsDelete IsDeleted { get; set; }
    }
}