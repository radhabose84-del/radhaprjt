using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Language.Queries.GetLanguages
{
    public class LanguageDTO
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Status IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}