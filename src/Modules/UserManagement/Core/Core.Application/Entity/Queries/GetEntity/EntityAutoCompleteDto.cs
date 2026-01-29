using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Entity.Queries.GetEntity
{
    public class EntityAutoCompleteDto
    {
        public int Id { get; set; }
        public string? EntityName { get; set; }
    }
}