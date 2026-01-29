using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.Application.MiscMaster.Command.DeleteMiscMaster
{
    public class DeleteMiscMasterCommand : IRequest<bool>
    {
          public int Id { get; set; }
    }
}