#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Lookups.Users
{
    public class CompanyLookupDto
    {        
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = default!;
        public string LegalName { get; set; } = default!;
        public string GstNumber { get; set;  }
        public string TinNumber { get; set;  }
        public string TanNumber { get; set; } = default!;
        public string CstNumber { get; set;  }
        public int EntityId { get; set;  }
    }
}