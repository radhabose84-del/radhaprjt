using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineLineNo
{
    public class GetMachineLinenoQuery : IRequest<ApiResponseDTO<List<GetMiscMasterDto>>> 
    {
        
    }
}