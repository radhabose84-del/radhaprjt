using AutoMapper;
using Core.Application.Divisions.Commands.CreateDivision;
using Core.Application.Divisions.Commands.DeleteDivision;
using Core.Application.Divisions.Commands.UpdateDivision;
using Core.Application.Divisions.Queries.GetDivisions;
using Core.Application.Users.Queries.GetUsers;
using Core.Domain.Entities;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class DivisionProfile : Profile
    {
        public DivisionProfile()
        {
            CreateMap<CreateDivisionCommand, Division>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            CreateMap<UpdateDivisionCommand, Division>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));
            CreateMap<DeleteDivisionCommand, Division>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
            CreateMap<Division, DivisionDTO>();            
            CreateMap<Division, DivisionAutoCompleteDTO>();
            // CreateMap<UserCompanyDTO, UserCompany>();
        }
    }
}