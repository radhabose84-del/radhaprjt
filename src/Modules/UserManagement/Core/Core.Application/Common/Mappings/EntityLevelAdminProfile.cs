using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.EntityLevelAdmin.Commands.CreateEntityLevelAdmin;
using Core.Application.EntityLevelAdmin.Commands.ResetPassword;
using Core.Domain.Entities;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class EntityLevelAdminProfile : Profile
    {
        public EntityLevelAdminProfile()
        {
            CreateMap<CreateEntityLevelAdminCommand,User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.EmailId, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsFirstTimeUser, opt => opt.MapFrom(src => FirstTimeUserStatus.Yes))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<ResetPasswordCommand, User>()
            .ForMember(dest => dest.UserName, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)))
            .ForMember(dest => dest.IsFirstTimeUser, opt => opt.MapFrom(src => FirstTimeUserStatus.No));
        }
    }
}