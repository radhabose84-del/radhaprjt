#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.SwitchProfile.Queries.GetUnitProfile;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Mappings
{
    public class SwitchProfile : Profile
    {
        public SwitchProfile()
        {
             CreateMap<Unit, GetUnitProfileDTO>()
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DivisionName, opt => opt.MapFrom(src => src.Division.Name))
            .ForMember(dest => dest.DivisionShortName, opt => opt.MapFrom(src => src.Division.ShortName))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CompanyName));
        }
    }
}