using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace Core.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
          public int Id { get; set; }
        
    }
}