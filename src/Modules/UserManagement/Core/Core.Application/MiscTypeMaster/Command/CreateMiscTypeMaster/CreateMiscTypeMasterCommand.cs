using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace Core.Application.MiscTypeMaster.Command.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommand : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
      
       public string? MiscTypeCode { get; set; }
       public string? Description { get; set; }
        
    }
}