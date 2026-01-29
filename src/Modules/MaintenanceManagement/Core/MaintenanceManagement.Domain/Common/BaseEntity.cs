namespace MaintenanceManagement.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public enum Status
        {
            Inactive = 0,
            Active  = 1
        }
        public enum IsDelete
        {
            NotDeleted = 0,
            Deleted = 1
        }    
         // Enum properties
        public Status IsActive { get; set; } = Status.Active; // Default Active
        public IsDelete IsDeleted { get; set; } = IsDelete.NotDeleted; // Default NotDeleted
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