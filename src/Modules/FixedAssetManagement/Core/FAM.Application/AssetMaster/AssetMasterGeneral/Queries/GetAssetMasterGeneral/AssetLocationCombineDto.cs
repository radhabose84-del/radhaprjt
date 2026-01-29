using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral
{
    public class AssetLocationCombineDto
    {
        public int UnitId { get; set; } 
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public int SubLocationId { get; set; } 
        public int CustodianId { get; set; }
        public int UserId { get; set; } 
    }
}