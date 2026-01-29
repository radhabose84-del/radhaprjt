using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered
{
    public class AssetTransferIssueDtlDto
    {
    public int AssetId { get; set; }
    public decimal AssetValue { get; set; }
    }
}