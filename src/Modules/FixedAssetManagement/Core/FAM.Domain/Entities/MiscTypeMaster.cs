using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Common;

namespace FAM.Domain.Entities
{
    public class MiscTypeMaster :BaseEntity
    {
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }
        
        public ICollection<MiscMaster>? MiscMaster { get; set; }
    }
}