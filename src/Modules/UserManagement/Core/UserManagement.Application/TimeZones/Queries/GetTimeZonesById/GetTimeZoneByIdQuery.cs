using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.TimeZones.Queries.GetTimeZones;
using MediatR;

namespace UserManagement.Application.TimeZones.Queries.GetTimeZonesById
{
    public class GetTimeZoneByIdQuery :IRequest<TimeZonesDto>
    { 
        public int TimeZoneId { get; set; }
    }
}