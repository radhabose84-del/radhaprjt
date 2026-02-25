namespace MaintenanceManagement.Application.Reports.MaintenanceRequestReport
{
    public class RequestReportDto
    {
        public int RequestId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? RequestCreatedName { get; set; }

        public int MaintenanceDepartmentId { get; set; }
        public string? MaintenanceDepartment { get; set; }

        public int ProductionDepartmentId { get; set; }
        public string? ProductionDepartment { get; set; }
        // public int DepartmentId { get; set; }
        // public string? Department { get; set; }
        public int? MachineId { get; set; }
        public string? MachineName { get; set; }
        public int? MaintenanceTypeId { get; set; }
        public string? MaintenanceType { get; set; }
        public int? WorkOrderId { get; set; }
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? OldVendorId { get; set; }
        public string? OldVendorName { get; set; }

        public DateTimeOffset? ExpectedDispatchDate { get; set; }
        public decimal? EstimatedSpareCost { get; set; }
        public decimal? EstimatedServiceCost { get; set; }

        public int? ServiceLocationId { get; set; }

        public string? ServiceLocation { get; set; }
        public int? ServiceTypeId { get; set; }
        public string? ServiceType { get; set; }
        public int? SparesTypeId { get; set; }
        public string? SparesType { get; set; }
        public int? ModeOfDispatchId { get; set; }
        public string? ModeOfDispatch { get; set; }
        public string? RequestMinutesDifference { get; set; }

        public DateTimeOffset? DowntimeStart { get; set; }
        public DateTimeOffset? DowntimeEnd { get; set; }
        public string? DownTime { get; set; }
        public string? TimeTakenToRepair { get; set; }
        
        public string? Remarks { get; set; }

       

                


           
           

        






    }
}