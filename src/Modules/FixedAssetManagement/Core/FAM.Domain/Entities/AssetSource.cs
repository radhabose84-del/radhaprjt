using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetPurchase;

namespace FAM.Domain.Entities
{
    public class AssetSource : BaseEntity
    { 
        public string? SourceCode { get; set; }
        public string? SourceName { get; set; }
        public ICollection<FAM.Domain.Entities.AssetPurchase.AssetPurchaseDetails>? AssetPurchase { get; set; }   
        public ICollection<AssetAdditionalCost>? AssetAdditionalCost { get; set; }

    }
}