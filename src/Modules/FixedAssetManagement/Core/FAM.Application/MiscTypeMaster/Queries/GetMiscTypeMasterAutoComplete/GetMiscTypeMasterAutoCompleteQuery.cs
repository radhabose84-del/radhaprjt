using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;

namespace FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery  :  IRequest<List<GetMiscTypeMasterAutocompleteDto>>
    {
          public string? SearchPattern { get; set; }
    }
}