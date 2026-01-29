
namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete
{
    public class MachineGroupUserAutoCompleteDto
    {
        public int Id { get; set; }
        public string? GroupName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? UserName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}