using AutoMapper;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;

namespace PurchaseManagement.Application.Common.Mappings.Quotation
{
    public class QuotationCompareMappingProfile : Profile
    {
        public QuotationCompareMappingProfile()
        {
              // Header mapping
            CreateMap<CreateQuoteComparsionDto, QuotationComparisonHeader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id is DB generated
                .ForMember(dest => dest.QuotationConfirmedDetails, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.ConfirmedDate, opt => opt.Ignore()) // set in handler
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())   // set in handler
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())     // set in handler
                .ForMember(dest => dest.StatusId, opt => opt.Ignore());     // can be set later

            // Detail mapping
            CreateMap<CreateQuoteComparsionDto.CreateQuoteComparsionDetailDto, QuotationComparisonDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id is DB generated
                .ForMember(dest => dest.QuotationComparisonHeaderId, opt => opt.Ignore())
                .ForMember(dest => dest.OverrideStatus, opt => opt.MapFrom(src => src.OverrideStatus == 1 ? true : false)); // EF will handle
             
        }
    }
}