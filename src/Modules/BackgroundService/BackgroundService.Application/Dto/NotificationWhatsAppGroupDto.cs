namespace BackgroundService.Application.Dto
{
    // Used by GetAll and GetById
    public class NotificationWhatsAppGroupDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;       
        public string ApiKey { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    // Used by autocomplete 
    public class NotificationWhatsAppGroupAutoCompleteDto
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class NotificationWhatsAppGroupListFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize  { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public int? DepartmentId { get; set; }
    }
}
