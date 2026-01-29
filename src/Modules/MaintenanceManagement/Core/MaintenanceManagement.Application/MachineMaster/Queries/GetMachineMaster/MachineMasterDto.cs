using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster
{
    public class MachineMasterDto
    {
        public int Id { get; set; }
        public string? MachineCode { get; set; }
        public string? MachineName { get; set; }
        public int MachineGroupId { get; set; }
        public string? MachineGroupName { get; set; }
        public int UnitId { get; set; }
        public decimal? ProductionCapacity { get; set; }
        public int UomId { get; set; }
        public int ShiftMasterId { get; set; }
        public int CostCenterId { get; set; }
        public int WorkCenterId { get; set; }
        public DateTimeOffset? InstallationDate { get; set; }
        public int AssetId { get; set; }
        public int LineNo { get; set; }
        public Status IsActive { get; set; }
        public byte IsProductionMachine { get; set; }
        public int DepartmentId { get; set; }
        public string? ProductionDepartmentName { get; set; }
        public string? SpecificationName { get; set; }

        
    }
}