using AutoMapper;
using PurchaseManagement.Application.OCREntry.Commands.CreateOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.UpdateOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class OCREntryProfile : Profile
    {
        public OCREntryProfile()
        {
            // Child rows are built explicitly in the handler (QualityTemplateId stamped per row),
            // so the header maps ignore the OcrQualityParameters navigation.
            CreateMap<CreateOCREntryCommand, Domain.Entities.OCREntry>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.OcrQualityParameters, opt => opt.Ignore());

            CreateMap<UpdateOCREntryCommand, Domain.Entities.OCREntry>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.OcrQualityParameters, opt => opt.Ignore());

            CreateMap<OCRQualityParameterInputDto, Domain.Entities.OCRQualityParameter>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
        }
    }
}
