using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace FAM.Application.MiscTypeMaster.Command.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommand : IRequest<GetMiscTypeMasterDto>
    {
      
       public string? MiscTypeCode { get; set; }
       public string? Description { get; set; }
        
    }
}