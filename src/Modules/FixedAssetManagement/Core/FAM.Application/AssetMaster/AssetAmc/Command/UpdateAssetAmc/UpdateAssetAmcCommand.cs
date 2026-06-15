using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetAmc.Command.UpdateAssetAmc
{
    public class UpdateAssetAmcCommand :IRequest<int>, IRequirePermission
    {
    
        public int Id {get;set;}
        public int AssetId { get; set; }
        public DateOnly? StartDate { get; set; }
        public int? Period { get; set; }   
        public string? VendorCode { get; set; }  
        public string? VendorName { get; set; }  
        public string? VendorPhone { get; set; }  
        public string? VendorEmail { get; set; }  
        public int? CoverageType { get; set; } 
        public int? FreeServiceCount  { get; set; }  
        public int? RenewalStatus { get; set; }  
        public DateOnly? RenewedDate { get; set; }  
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
