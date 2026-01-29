using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.Dashboard.CardView
{
    public class AssetDashboardDto
    {
        public CardViewDto CardView { get; set; } = new();
       public List<AssetGroupSummaryDto> GroupSummary { get; set; } = new();
    }
}