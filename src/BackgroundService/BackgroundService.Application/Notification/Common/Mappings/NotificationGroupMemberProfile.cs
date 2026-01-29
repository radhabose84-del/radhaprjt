using AutoMapper;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.CreateNotificationGroupMember;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.UpdateNotificationGroupMember;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetAllNotificationGroupMember;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Application.Notification.Common.Mappings
{
    public class NotificationGroupMemberProfile : Profile
    {
        public NotificationGroupMemberProfile()
        {
            CreateMap<Domain.Entities.Notification.NotificationGroupMembers, NotificationGroupMemberDto>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0));

            CreateMap<CreateNotificationGroupMemberCommand, Domain.Entities.Notification.NotificationGroupMembers>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateNotificationGroupMemberCommand, Domain.Entities.Notification.NotificationGroupMembers>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));   
            
            CreateMap<Domain.Entities.Notification.NotificationGroup, GetNotificationGroupMemberDto>()
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.Users, opt => opt.Ignore());
        }
    }
}