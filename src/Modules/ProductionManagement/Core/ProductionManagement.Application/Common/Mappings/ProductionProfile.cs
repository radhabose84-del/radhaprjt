using AutoMapper;
using ProductionManagement.Application.ProductionPack.Dto;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class ProductionProfile : Profile
    {
        public ProductionProfile()
        {
            // Detail item → Detail entity
            CreateMap<CreateProductionDetailItem, ProductionPackEntryDetail>();
            CreateMap<UpdateProductionDetailItem, ProductionPackEntryDetail>();

            // Create: DTO → header entity with Details collection
            CreateMap<CreateProductionDto, ProductionPackEntry>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PackNo, opt => opt.Ignore())
                .ForMember(dest => dest.UnitId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details));

            // Update: DTO → header entity with Details collection
            CreateMap<UpdateProductionDto, ProductionPackEntry>()
                .ForMember(dest => dest.PackNo, opt => opt.Ignore())
                .ForMember(dest => dest.UnitId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details));

            // Autocomplete
            CreateMap<ProductionLookupDto, ProductionLookupDto>();
        }
    }
}
