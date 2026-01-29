using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace MaintenanceManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand  : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
          public int Id { get; set; }
    }
}