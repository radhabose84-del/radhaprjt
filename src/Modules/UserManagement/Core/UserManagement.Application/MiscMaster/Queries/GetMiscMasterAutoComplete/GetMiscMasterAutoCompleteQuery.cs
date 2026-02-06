using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace UserManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public class GetMiscMasterAutoCompleteQuery :  IRequest<List<GetMiscMasterAutoCompleteDto>>
    {

          public string? SearchPattern { get; set; }
          public string? MiscTypeCode { get; set; }
        
    }
}