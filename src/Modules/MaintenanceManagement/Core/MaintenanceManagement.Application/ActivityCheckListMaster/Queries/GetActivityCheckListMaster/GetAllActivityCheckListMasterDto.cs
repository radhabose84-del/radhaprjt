namespace MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster
{
    public class GetAllActivityCheckListMasterDto
    {
        
        public int ChecklistId { get; set; }
        public int ActivityID { get; set; }
        public string? ActivityName { get; set; }
        public string? ActivityChecklist { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int DepartmentId { get; set; }    
        public string? DepartmentName { get; set; }   
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}