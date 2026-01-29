using AutoMapper;
using Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;


namespace Core.Application.Common.Mappings
{
    public class UserRoleAllocationProfile : Profile
    {
        public UserRoleAllocationProfile()
        {
            CreateMap<Core.Domain.Entities.UserRoleAllocation, UserRoleAllocationResponseDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.UserRole.RoleName));

            CreateMap<UserRoleAllocationResponseDto, CreateUserRoleAllocationDto>()
            .ForMember(dest => dest.RoleIds, opt => opt.MapFrom(src => new List<int> { src.UserRoleId }));
        }
    }
}