#nullable disable
using AutoMapper;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;


namespace UserManagement.Application.Common.Mappings
{
    public class UserRoleAllocationProfile : Profile
    {
        public UserRoleAllocationProfile()
        {
            CreateMap<UserManagement.Domain.Entities.UserRoleAllocation, UserRoleAllocationResponseDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.UserRole.RoleName));

            CreateMap<UserRoleAllocationResponseDto, CreateUserRoleAllocationDto>()
            .ForMember(dest => dest.RoleIds, opt => opt.MapFrom(src => new List<int> { src.UserRoleId }));
        }
    }
}