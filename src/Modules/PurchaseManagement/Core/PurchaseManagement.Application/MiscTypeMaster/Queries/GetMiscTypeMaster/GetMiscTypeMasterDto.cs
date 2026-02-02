using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster
{
    public class GetMiscTypeMasterDto
    {
        public int Id { get; set; }
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; } 
        public Status  IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
    }
}