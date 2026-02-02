using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO
{
    public class UnitPriceDto
    {
         public int ItemId { get; set; }
         public int PriceMasterHeaderId { get; set; }
         public decimal UnitPrice { get; set; }
    }
}