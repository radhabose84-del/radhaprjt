using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.Power;

namespace MaintenanceManagement.Domain.Entities
{
    public class MachineMaster : BaseEntity
    {
        public string? MachineCode { get; set; }
        public string? MachineName { get; set; }
        public int MachineGroupId { get; set; }
        public MachineGroup MachineGroup { get; set; } = null!;
        public int UnitId { get; set; }
        public decimal? ProductionCapacity { get; set; }
        public int UomId { get; set; }
        public int ShiftMasterId { get; set; }
        public ShiftMaster ShiftMaster { get; set; } = null!;
        public int CostCenterId { get; set; }
        public CostCenter CostCenter { get; set; } = null!;
        public int WorkCenterId { get; set; }
        public WorkCenter WorkCenter { get; set; } = null!;
        public DateTimeOffset? InstallationDate { get; set; }

        public int AssetId { get; set; }
        public ICollection<PreventiveSchedulerDetail>? PreventiveSchedulerDetail { get; set; }

        public ICollection<MaintenanceRequest> MaintenanceRequest { get; set; } = null!;
        public int LineNo { get; set; }
        public MiscMaster LineNoMachine { get; set; } = null!;
        public bool IsProductionMachine { get; set; }
        public ICollection<MachineSpecification> MachineSpecification { get; set; } = null!;
        public ICollection<GeneratorConsumption>? GeneratorConsumption { get; set; }  
                    

    }
}