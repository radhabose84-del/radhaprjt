namespace MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMaster
{
    public class ShiftMasterDTO
    {
        public int Id { get; set; }
        public string ShiftCode { get; set; } = default!;
        public string ShiftName { get; set; } = default!;
        public DateOnly EffectiveDate { get; set; }
        public DateTimeOffset CreatedDate { get; set; } // Use DateTimeOffset for accurate date
    }
}