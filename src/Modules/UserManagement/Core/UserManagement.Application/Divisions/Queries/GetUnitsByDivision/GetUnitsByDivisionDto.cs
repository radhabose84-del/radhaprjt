namespace UserManagement.Application.Divisions.Queries.GetUnitsByDivision
{
    public class GetUnitsByDivisionDto
    {
        public int CompanyId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int UnitTypeId { get; set; }
        public string? UnitTypeName { get; set; }
        public int DivisionId { get; set; }
        public string? DivisionName { get; set; }
    }
}
