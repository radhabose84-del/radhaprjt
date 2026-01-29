using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class MiscTypeMaster :BaseEntity
    {
        public int Id { get; set; }
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }
        
        public ICollection<MiscMaster>? MiscMaster { get; set; }
    }
}