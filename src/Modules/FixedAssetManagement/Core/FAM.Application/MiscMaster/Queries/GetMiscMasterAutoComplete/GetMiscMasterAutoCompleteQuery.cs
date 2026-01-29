using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public class GetMiscMasterAutoCompleteQuery  :  IRequest<List<GetMiscMasterAutoCompleteDto>>
    {


          public string? MiscTypeCode { get; set; }
         public string? MiscTypeName { get; set; }
        
    }
}