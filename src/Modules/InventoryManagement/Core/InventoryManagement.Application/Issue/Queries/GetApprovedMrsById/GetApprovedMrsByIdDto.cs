using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Issue.Queries.GetApprovedMrsById
{
    public class GetApprovedMrsByIdDto
    {
         public int MrsId { get; set; }
        public string? MrsNo { get; set; }
    }
}