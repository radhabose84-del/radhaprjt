using AutoMapper;
using SalesManagement.Application.Complaint.Commands.CreateComplaint;
using SalesManagement.Application.Complaint.Commands.UpdateComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class ComplaintProfile : Profile
    {
        public ComplaintProfile()
        {
            // Create Command → Entity
            CreateMap<CreateComplaintCommand, ComplaintHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.ComplaintDetails, opt => opt.Ignore());

            CreateMap<CreateComplaintDetailDto, ComplaintDetail>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.ComplaintDetailNatures, opt => opt.Ignore());

            // Update Command → Entity
            CreateMap<UpdateComplaintCommand, ComplaintHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.ComplaintDetails, opt => opt.Ignore());
        }
    }
}
