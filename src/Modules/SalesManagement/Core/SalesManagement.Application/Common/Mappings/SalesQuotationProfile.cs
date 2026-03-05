using AutoMapper;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.UpdateSalesQuotation;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesQuotationProfile : Profile
    {
        public SalesQuotationProfile()
        {
            // Create: Command → Header entity (with nested details)
            CreateMap<CreateSalesQuotationCommand, SalesQuotationHeader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SalesQuotationDetails, opt => opt.MapFrom(src => src.SalesQuotationDetails))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Create: Detail DTO → Detail entity
            CreateMap<CreateSalesQuotationDetailDto, SalesQuotationDetail>();

            // Update: Command → Header entity
            CreateMap<UpdateSalesQuotationCommand, SalesQuotationHeader>()
                .ForMember(dest => dest.SalesQuotationDetails, opt => opt.MapFrom(src => src.SalesQuotationDetails))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            // Update: Detail DTO → Detail entity
            CreateMap<UpdateSalesQuotationDetailDto, SalesQuotationDetail>();
        }
    }
}
