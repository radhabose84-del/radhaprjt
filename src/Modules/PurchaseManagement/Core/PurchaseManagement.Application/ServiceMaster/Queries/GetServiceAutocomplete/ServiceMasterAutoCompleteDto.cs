namespace PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete
{
    public class ServiceMasterAutoCompleteDto
    {

        public int Id { get; set; }
        public string? ServiceCode { get; set; }
        public string? ServiceDescription { get; set; }
        public int SacId { get; set; }
        public string? SacName { get; set; }
        public decimal? GstPercentage { get; set; }
        public int UomId { get; set; }
        public string? UomName { get; set; } 
        public int ServiceCategoryId { get; set; }
        public string? ServiceCategory { get; set; }
        
    }
}