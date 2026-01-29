using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById
{
    public class AssetAMCDTOById
    {
        public int Id { get; set; }
         public string? StartDate { get; set; }
         public string? EndDate { get; set; }
         public int Period { get; set; }
         public string? VendorCode { get; set; }
         public string? VendorName { get; set; }
         public string? CoverageType { get; set; }
         public string? RenewalStatus { get; set; }
         public string? RenewedDate { get; set; }
         public string? AssetName { get; set; }
         public int CoverageTypeId { get; set; }
         public int RenewalStatusId { get; set; }
         public byte IsActive { get; set; }
         public int FreeServiceCount { get; set; }
        public string? VendorPhone { get; set; }  
        public string? VendorEmail { get; set; }  
    }
}