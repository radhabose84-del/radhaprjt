using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Units.Queries.GetUnits
{
    public class UnitAutoCompleteDTO
    {
        public int Id { get; set; }
        public string? UnitName { get; set; }
        public int DivisionId { get; set; }
    }
}