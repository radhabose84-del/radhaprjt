namespace MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterAutoComplete
{
    public class ShiftMasterAutoCompleteDTO
    {
        public int Id { get; set; }
        public string ShiftCode { get; set; } = default!;
        public string ShiftName { get; set; } = default!;
    }
}