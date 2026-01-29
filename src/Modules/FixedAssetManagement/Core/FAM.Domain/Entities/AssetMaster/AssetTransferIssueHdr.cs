using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Common;

namespace FAM.Domain.Entities.AssetMaster
{
    public class AssetTransferIssueHdr 
    {
        public int Id {get;set;}
        public DateTimeOffset DocDate { get; set; }
        public int? TransferType { get; set; }        
        public MiscMaster TransferTypeIssueMiscType { get; set; } = null!;  
        public int FromUnitId { get; set; }  
        public int ToUnitId { get; set; } 
        public int FromDepartmentId  { get; set; } 
        public int ToDepartmentId  { get; set; } 
        public int FromCustodianId  { get; set; } 
        public string? FromCustodianName { get; set; }
        public int ToCustodianId  { get; set; } 
        public string? ToCustodianName { get; set; }
        public string? Status { get; set; } 
        public byte AckStatus { get; set; }
        public string? GatePassNo { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        public int? AuthorizedBy { get; set; }
        public DateTimeOffset? AuthorizedDate { get; set; }
        public string? AuthorizedByName { get; set; }
        public string? AuthorizedIP { get; set; }

    
        public ICollection<AssetTransferIssueDtl>? AssetTransferIssueDtl { get; set; } 
    

         public AssetTransferReceiptHdr? AssetTransferReceiptHdr { get; set; } 
 
       
    }
}