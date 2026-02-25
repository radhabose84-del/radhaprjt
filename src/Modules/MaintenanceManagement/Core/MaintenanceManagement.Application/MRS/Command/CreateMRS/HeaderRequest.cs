namespace MaintenanceManagement.Application.MRS.Command.CreateMRS
{
    public class HeaderRequest
    {
    public string? Divcode { get; set; }
    public DateTime IrDate { get; set; }
    public string?  Depcode { get; set; }
    public string? SubDepcode { get; set; }
    public string? Refno { get; set; }
    public string? MaintenanceType { get; set; }
    public string? Remarks { get; set; }
    public List<DetailRequest>? Details { get; set; }
      

    }
}