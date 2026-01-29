using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetClosingReaderValueById
{
    public class GetClosingReaderValueByIdQuery :  IRequest<GetClosingReaderValueDto>
    {
        public int FeederId { get; set; }
    }
}