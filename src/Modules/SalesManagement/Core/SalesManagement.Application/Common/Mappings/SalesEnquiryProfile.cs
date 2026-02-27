using AutoMapper;
using SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.UpdateSalesEnquiry;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;
using static SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry.CreateSalesEnquiryDto;
using static SalesManagement.Application.SalesEnquiry.Commands.UpdateSalesEnquiry.UpdateSalesEnquiryCommand;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesEnquiryProfile : Profile
    {
        public SalesEnquiryProfile()
        {
            // Create: DTO → Header entity (with nested details)
            CreateMap<CreateSalesEnquiryDto, SalesEnquiryHeader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SalesEnquiryDetails, opt => opt.MapFrom(src => src.SalesEnquiryDetails))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Create: Detail DTO → Detail entity
            CreateMap<CreateSalesEnquiryDetailDto, SalesEnquiryDetail>();

            // Update: Command → Header entity
            CreateMap<UpdateSalesEnquiryCommand, SalesEnquiryHeader>()
                .ForMember(dest => dest.SalesEnquiryDetails, opt => opt.MapFrom(src => src.SalesEnquiryDetails))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            // Update: Detail DTO → Detail entity
            CreateMap<UpdateSalesEnquiryDetailDto, SalesEnquiryDetail>();
        }
    }
}
