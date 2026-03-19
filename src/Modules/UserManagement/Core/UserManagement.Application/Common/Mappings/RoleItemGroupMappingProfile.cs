using AutoMapper;
using UserManagement.Application.RoleItemGroupMapping.Commands.CreateRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.UpdateRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.DeleteRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class RoleItemGroupMappingProfile : Profile
    {
        public RoleItemGroupMappingProfile()
        {
            CreateMap<CreateRoleItemGroupMappingCommand, Domain.Entities.RoleItemGroupMapping>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateRoleItemGroupMappingCommand, Domain.Entities.RoleItemGroupMapping>()
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteRoleItemGroupMappingCommand, Domain.Entities.RoleItemGroupMapping>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<Domain.Entities.RoleItemGroupMapping, RoleItemGroupMappingDto>();
            CreateMap<Domain.Entities.RoleItemGroupMapping, RoleItemGroupMappingLookupDto>();
        }
    }
}
