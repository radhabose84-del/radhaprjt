using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.Common.HttpResponse;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace ProjectManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommand : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
      
       public string? MiscTypeCode { get; set; }
       public string? Description { get; set; }
        
        
    }
}