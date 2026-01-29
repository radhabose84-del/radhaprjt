using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.MiscTypeMaster.Queries.GetMiscTypeMaster
{
    public class GetMiscTypeMasterAutocompleteDto
    {
          public int Id { get; set; }
        public string? MiscTypeCode { get; set; }
    }
}