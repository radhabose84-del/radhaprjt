namespace UserManagement.Application.Units.Queries.GetUnits
{
    public class UnitAutoCompleteDTO
    {
        public int Id { get; set; }
        public string? UnitName { get; set; }
        public int DivisionId { get; set; }
        public int UnitTypeId { get; set; }
        public string? UnitTypeName { get; set; }
        public int? PinCode { get; set; }
    }
}