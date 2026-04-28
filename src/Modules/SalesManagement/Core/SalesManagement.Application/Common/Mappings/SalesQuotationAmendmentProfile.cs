using AutoMapper;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesQuotationAmendmentProfile : Profile
    {
        public SalesQuotationAmendmentProfile()
        {
            // Amendment Header: entity → DTO
            CreateMap<SalesQuotationAmendmentHeader, SalesQuotationAmendmentHeaderDto>()
                .ForMember(dest => dest.QuotationNo,
                    opt => opt.MapFrom(src => src.SalesQuotationHeader != null ? src.SalesQuotationHeader.QuotationNo : null))
                .ForMember(dest => dest.StatusName,
                    opt => opt.MapFrom(src => src.StatusMisc != null ? src.StatusMisc.Description : null))
                .ForMember(dest => dest.SalesQuotationAmendmentDetails,
                    opt => opt.MapFrom(src => src.SalesQuotationAmendmentDetails));

            // Amendment Detail: entity → DTO (lookup names populated in query repository)
            CreateMap<SalesQuotationAmendmentDetail, SalesQuotationAmendmentDetailDto>()
                .ForMember(dest => dest.OldItemName, opt => opt.Ignore())
                .ForMember(dest => dest.OldHSNCode, opt => opt.Ignore())
                .ForMember(dest => dest.NewItemName, opt => opt.Ignore())
                .ForMember(dest => dest.NewHSNCode, opt => opt.Ignore())
                .ForMember(dest => dest.Remarks, opt => opt.Ignore());
        }
    }
}
