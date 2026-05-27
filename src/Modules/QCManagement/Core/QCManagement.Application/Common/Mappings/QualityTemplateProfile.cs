using AutoMapper;
using QCManagement.Application.QualityTemplate.Dto;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Application.Common.Mappings
{
    public class QualityTemplateProfile : Profile
    {
        public QualityTemplateProfile()
        {
            // Entity → DTO (Parameters collection filled by Dapper second query)
            CreateMap<Domain.Entities.QualityTemplate, QualityTemplateDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted))
                .ForMember(dest => dest.Parameters, opt => opt.Ignore());

            // Entity → Lookup DTO
            CreateMap<Domain.Entities.QualityTemplate, QualityTemplateLookupDto>();

            // Command → Entity (children mapped manually in handler)
            CreateMap<QualityTemplate.Commands.CreateQualityTemplate.CreateQualityTemplateCommand, Domain.Entities.QualityTemplate>()
                .ForMember(dest => dest.TemplateCode, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.QualityTemplateParameters, opt => opt.Ignore());

            CreateMap<QualityTemplate.Commands.UpdateQualityTemplate.UpdateQualityTemplateCommand, Domain.Entities.QualityTemplate>()
                .ForMember(dest => dest.TemplateCode, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.QualityTemplateParameters, opt => opt.Ignore());
        }
    }
}
