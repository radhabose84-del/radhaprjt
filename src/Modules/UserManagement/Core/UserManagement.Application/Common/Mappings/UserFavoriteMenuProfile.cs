using AutoMapper;
using UserManagement.Application.UserFavoriteMenu.Commands.AddUserFavoriteMenu;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class UserFavoriteMenuProfile : Profile
    {
        public UserFavoriteMenuProfile()
        {
            CreateMap<AddUserFavoriteMenuCommand, Domain.Entities.UserFavoriteMenu>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
        }
    }
}
