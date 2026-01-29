using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Core.Application.Common.Mappings
{
    public class TimeZonesProfile : Profile
    {
        public TimeZonesProfile()
        {
            CreateMap<Core.Domain.Entities.TimeZones, Core.Application.TimeZones.Queries.GetTimeZones.TimeZonesDto>();
            CreateMap<Core.Domain.Entities.TimeZones, Core.Application.TimeZones.Queries.GetTimeZones.TimeZonesAutoCompleteDto>();
        }
    }
}