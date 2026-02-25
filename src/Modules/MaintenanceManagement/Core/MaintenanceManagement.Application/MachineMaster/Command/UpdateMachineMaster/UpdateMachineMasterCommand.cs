using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Command.UpdateMachineMaster
{
    public class UpdateMachineMasterCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string? MachineCode { get; set; }
        public string? MachineName { get; set; }
        public int MachineGroupId { get; set; }
        public int UnitId { get; set; }
        public decimal? ProductionCapacity { get; set; }
        public int UomId { get; set; }
        public int ShiftMasterId { get; set; }
        public int CostCenterId { get; set; }
        public int WorkCenterId { get; set; }
        public DateTimeOffset? InstallationDate { get; set; }
        public int AssetId { get; set; }
        public int LineNo { get; set; }
        public byte IsActive { get; set; }
        public byte IsProductionMachine { get; set; }
        
    }
}