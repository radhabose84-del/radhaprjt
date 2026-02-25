using AutoMapper;
using UserManagement.Application.Users.Commands.ChangeUserPassword;
using UserManagement.Application.Users.Commands.UpdateFirstTimeUserPassword;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Mappings
{
    public class ChangePasswordProfile : Profile
    {
        public ChangePasswordProfile()
        {
             CreateMap<FirstTimeUserPasswordCommand, PasswordLog>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));

            CreateMap<ChangeUserPasswordCommand, PasswordLog>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.NewPassword));
        }
    }
}