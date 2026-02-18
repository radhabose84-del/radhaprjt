using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetLocation.Commands.UpdateAssetLocation
{
    public class UpdateAssetLocationCommand :   IRequest<int>
    {

        public int AssetId { get; set; }
         public int UnitId { get; set; } 
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public int SubLocationId { get; set; } 
        public int CustodianId { get; set; }
        public int UserID { get; set; }     
    }
}