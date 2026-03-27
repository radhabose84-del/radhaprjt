using AutoMapper;
using SalesManagement.Application.ComplaintQCReview.Commands.SubmitQCReview;
using SalesManagement.Application.ComplaintQCReview.Commands.UpdateQCReview;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class ComplaintQCReviewProfile : Profile
    {
        public ComplaintQCReviewProfile()
        {
            // Submit Command → Entity
            CreateMap<SubmitQCReviewCommand, Domain.Entities.ComplaintQCReview>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.Assignments, opt => opt.Ignore());

            // Update Command → Entity
            CreateMap<UpdateQCReviewCommand, Domain.Entities.ComplaintQCReview>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.Assignments, opt => opt.Ignore());
        }
    }
}
