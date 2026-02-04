using AutoMapper;
using UserManagement.Application.Divisions.Commands.CreateDivision;
using UserManagement.Application.Divisions.Commands.DeleteDivision;
using UserManagement.Application.Divisions.Commands.UpdateDivision;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Users.Queries.GetUsers;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
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