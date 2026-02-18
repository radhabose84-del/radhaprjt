using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetClosingReaderValueById
{
    public class GetClosingReaderValueByIdQuery :  IRequest<GetClosingReaderValueDto>
    {
        public int FeederId { get; set; }
    }
}