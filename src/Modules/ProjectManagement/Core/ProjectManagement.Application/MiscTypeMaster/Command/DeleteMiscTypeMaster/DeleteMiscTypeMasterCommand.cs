using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.Common.HttpResponse;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace ProjectManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
          public int Id { get; set; }
    }
}