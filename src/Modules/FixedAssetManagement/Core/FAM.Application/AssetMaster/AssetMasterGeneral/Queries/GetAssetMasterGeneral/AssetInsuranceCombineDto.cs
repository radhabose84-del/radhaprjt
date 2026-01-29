using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral
{
    public class AssetInsuranceCombineDto
    {
        public string? PolicyNo { get; set; }       
        public DateOnly? StartDate { get; set; }        
        public DateOnly? EndDate { get; set; }
        public string? PolicyAmount { get; set; }
        public string? VendorCode { get; set; }
    }
}