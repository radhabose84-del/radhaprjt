using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue
{
    public class UpdateAssetTransferDtlDto
    {
       public int AssetTransferId { get; set; }
        public int AssetId { get; set; }
       public decimal AssetValue { get; set; } 
    }
}