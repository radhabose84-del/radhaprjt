using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery  :  IRequest<GetMiscMasterDto>
    { 
         public int Id { get; set; }
    }
}