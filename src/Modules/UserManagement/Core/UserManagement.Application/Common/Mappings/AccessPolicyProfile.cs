using AutoMapper;
using UserManagement.Application.AccessPolicy.Commands.CreateAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.UpdateAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.DeleteAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.AssignRoleAccessPolicy;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class AccessPolicyProfile : Profile
    {
        public AccessPolicyProfile()
        {
            CreateMap<CreateAccessPolicyCommand, UserManagement.Domain.Entities.AccessPolicy>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateAccessPolicyCommand, UserManagement.Domain.Entities.AccessPolicy>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<AssignRoleAccessPolicyCommand, UserManagement.Domain.Entities.RoleAccessPolicy>();
        }
    }
}
