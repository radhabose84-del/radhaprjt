using AutoMapper;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.SubmitFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UpdateFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class ComplaintDepartmentFeedbackProfile : Profile
    {
        public ComplaintDepartmentFeedbackProfile()
        {
            // Submit Command → Entity
            CreateMap<SubmitComplaintDepartmentFeedbackCommand, Domain.Entities.ComplaintDepartmentFeedback>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            // Update Command → Entity
            CreateMap<UpdateComplaintDepartmentFeedbackCommand, Domain.Entities.ComplaintDepartmentFeedback>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            // SubmitAttachmentDto → Entity
            CreateMap<SubmitAttachmentDto, Domain.Entities.ComplaintFeedbackAttachment>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
        }
    }
}
