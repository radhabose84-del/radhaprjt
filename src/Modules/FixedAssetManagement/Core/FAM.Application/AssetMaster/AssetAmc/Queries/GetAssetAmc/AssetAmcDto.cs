using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc
{
    public class AssetAmcDto
    {
        public int Id {get; set;}
        public int AssetId { get; set; } 
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; } 
        public int? Period { get; set; }   
        public string? VendorCode { get; set; }  
        public string? VendorName { get; set; }  
        public string? VendorPhone { get; set; }  
        public string? VendorEmail { get; set; }  
        public int? CoverageType { get; set; } 
        public int? FreeServiceCount  { get; set; }  
        public int? RenewalStatus { get; set; }  
        public DateOnly? RenewedDate { get; set; }         
    }
}