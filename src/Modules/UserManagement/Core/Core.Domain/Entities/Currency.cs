using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;


namespace Core.Domain.Entities
{
    public class Currency : BaseEntity
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public CompanySettings? CompanySettings { get; set; }
    
    }
}