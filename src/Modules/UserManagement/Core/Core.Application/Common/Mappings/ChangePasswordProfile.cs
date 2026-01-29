using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Users.Commands.ChangeUserPassword;
using Core.Application.Users.Commands.UpdateFirstTimeUserPassword;
using Core.Domain.Entities;

namespace Core.Application.Common.Mappings
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