using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices
{
    public class GetServiceMasterDto
    {
       public   int Id { get; set; }
       public string? ServiceCode { get; set; }
       public  string? ServiceDescription { get; set; }
       public int SacId { get; set; }
       public string? SacName { get; set; }
       public int UomId { get; set; }
       public string? UomName { get; set; } 
       public int? ServiceCategoryId { get; set; }       
       public string? ServiceCategory { get; set; }
       public string? ServiceCategoryDescription { get; set; }
       public Status IsActive { get; set; }
       public  IsDelete IsDeleted { get; set; }
       public int CreatedBy { get; set; }
       public  DateTimeOffset? CreatedDate { get; set; }
       public string? CreatedByName { get; set; }
       public  string? CreatedIP { get; set; }
       public int? ModifiedBy { get; set; }
       public DateTimeOffset? ModifiedDate  { get; set; }
       public  string? ModifiedByName { get; set; }
       public  string? ModifiedIP  { get; set; }
    }
}