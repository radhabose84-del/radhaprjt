using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace Core.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public class GetMiscMasterAutoCompleteQuery :  IRequest<List<GetMiscMasterAutoCompleteDto>>
    {

          public string? SearchPattern { get; set; }
          public string? MiscTypeCode { get; set; }
        
    }
}