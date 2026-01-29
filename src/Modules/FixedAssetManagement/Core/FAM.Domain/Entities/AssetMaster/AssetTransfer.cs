using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Domain.Entities.AssetMaster
{
    public class AssetTransferIssue
    {

        public int assetId { get; set; }
        public AssetMasterGenerals? AssetMasterId  { get; set; }      
        public int AssetCategoryId { get; set; }
        public string? AssetName { get; set; }
        public int  UnitId { get; set; }
        public int DepartmentId { get; set; }
        public int CustodianId { get; set; }
        public int UserID { get; set; }
        public int LocationId { get; set; }
        public int SubLocationId { get; set; }

        public int assetTransferId { get; set; }


    }


}