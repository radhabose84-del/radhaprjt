using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.MiscTypeMaster.Queries.GetMiscTypeMaster
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