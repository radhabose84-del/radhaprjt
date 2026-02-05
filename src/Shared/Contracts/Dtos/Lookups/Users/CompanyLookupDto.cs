using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Lookups.Users
{
    public class CompanyLookupDto
    {        
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string LegalName { get; set; }
        public string GstNumber { get; set;  }
        public string TinNumber { get; set;  }
        public string TanNumber { get; set; } 
        public string CstNumber { get; set;  }
        public int EntityId { get; set;  }
    }
}