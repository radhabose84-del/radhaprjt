using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueById
{
    public class AssetTransferIssueByIdDto
    {
        public int Id { get; set; }
        public int AssetId {get;set;}
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; } 
        public decimal AssetValue { get; set; }
    }
}