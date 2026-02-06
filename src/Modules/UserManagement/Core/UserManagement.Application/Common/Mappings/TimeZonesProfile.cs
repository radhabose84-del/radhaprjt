using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace UserManagement.Application.Common.Mappings
{
    public class TimeZonesProfile : Profile
    {
        public TimeZonesProfile()
        {
            CreateMap<UserManagement.Domain.Entities.TimeZones, UserManagement.Application.TimeZones.Queries.GetTimeZones.TimeZonesDto>();
            CreateMap<UserManagement.Domain.Entities.TimeZones, UserManagement.Application.TimeZones.Queries.GetTimeZones.TimeZonesAutoCompleteDto>();
        }
    }
}