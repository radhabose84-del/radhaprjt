namespace InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterAutoComplete
{
    public class GetHSNMasterAutoCompleteDto
    {
        public int Id { get; set; }
        public string? HSNCode { get; set; }
        public string? HSNDescription { get; set; }
        
        public string? TypeCode { get; set; }
    }
}