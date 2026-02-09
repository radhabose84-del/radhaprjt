using System;

namespace Contracts.Dtos.Lookups.Maintenance
{
    public class CostCenterLookupDto
    {
        public int CostCenterId { get; set; }
        public string? CostCenterCode { get; set; }
        public string? CostCenterName { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
    }
}
