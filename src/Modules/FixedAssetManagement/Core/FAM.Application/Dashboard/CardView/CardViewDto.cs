using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.Dashboard.CardView
{
    public class CardViewDto
    {
        public int TotalAssets { get; set; }
        public decimal TotalAssetValue { get; set; }
        public int NewAssets { get; set; }
        public decimal NewAssetsValue { get; set; }
        public int AssetDisposed { get; set; }
    
    }
}