using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
        public class States : BaseEntity
        {
            public int Id { get; set; }
            public string? StateCode { get; set; }
            public string? StateName { get; set; }
            public int CountryId { get; set; }

            public Countries? Countries { get; set; }  
            public ICollection<Cities> Cities { get; set; } = new List<Cities>();
        }
}