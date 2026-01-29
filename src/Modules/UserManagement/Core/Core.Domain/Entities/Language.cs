using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;
using static Core.Domain.Enums.LanguageEnum;

namespace Core.Domain.Entities
{
    public class Language : BaseEntity
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public CompanySettings? CompanySettings { get; set; }
    }
}