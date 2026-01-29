using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Command.CreateAssetAmc
{
    public class CreateAssetAmcCommand :IRequest<int>
    {
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
    }
}