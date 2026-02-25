namespace MaintenanceManagement.Application.MRS.Command.CreateMRS
{
    public class DetailRequest
    {
    public string? ItemCode { get; set; }
    public string? Macno { get; set; }
    public decimal? QtyReqd { get; set; }
    public string? CatCode { get; set; }
    public string? CcCode { get; set; }
    public decimal CurrStk { get; set; }
    public decimal Rate { get; set; }
    
 
    }
}