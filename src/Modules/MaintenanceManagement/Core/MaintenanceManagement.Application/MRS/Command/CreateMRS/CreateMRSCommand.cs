using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Command.CreateMRS
{
    public class CreateMRSCommand :IRequest<int>
    {
         public HeaderRequest? Header { get; set; }
    }
}