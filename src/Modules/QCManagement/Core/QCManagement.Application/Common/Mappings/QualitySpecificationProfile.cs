using AutoMapper;
using QCManagement.Application.QualitySpecification.Dto;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Application.Common.Mappings
{
    public class QualitySpecificationProfile : Profile
    {
        public QualitySpecificationProfile()
        {
            // Entity → DTO (Parameters collection filled by Dapper second query)
            CreateMap<Domain.Entities.QualitySpecification, QualitySpecificationDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted))
                .ForMember(dest => dest.QualityTemplateCode, opt => opt.Ignore())
                .ForMember(dest => dest.QualityTemplateName, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicableLevelCode, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicableLevelName, opt => opt.Ignore())
                .ForMember(dest => dest.QcTypeCode, opt => opt.Ignore())
                .ForMember(dest => dest.QcTypeName, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCategoryName, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCode, opt => opt.Ignore())
                .ForMember(dest => dest.ItemName, opt => opt.Ignore())
                .ForMember(dest => dest.Parameters, opt => opt.Ignore());

            // Entity → Lookup DTO
            CreateMap<Domain.Entities.QualitySpecification, QualitySpecificationLookupDto>();

            // Command → Entity (children mapped manually in handler)
            CreateMap<QualitySpecification.Commands.CreateQualitySpecification.CreateQualitySpecificationCommand, Domain.Entities.QualitySpecification>()
                .ForMember(dest => dest.SpecificationCode, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.QualitySpecificationParameters, opt => opt.Ignore());

            CreateMap<QualitySpecification.Commands.UpdateQualitySpecification.UpdateQualitySpecificationCommand, Domain.Entities.QualitySpecification>()
                .ForMember(dest => dest.SpecificationCode, opt => opt.Ignore())
                .ForMember(dest => dest.QualityTemplateId, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicableLevelId, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.ItemId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.QualitySpecificationParameters, opt => opt.Ignore());
        }
    }
}
