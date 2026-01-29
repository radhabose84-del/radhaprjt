using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.TimeZones.Queries.GetTimeZones;
using MediatR;

namespace Core.Application.TimeZones.Queries.GetTimeZonesById
{
    public class GetTimeZoneByIdQuery :IRequest<TimeZonesDto>
    { 
        public int TimeZoneId { get; set; }
    }
}