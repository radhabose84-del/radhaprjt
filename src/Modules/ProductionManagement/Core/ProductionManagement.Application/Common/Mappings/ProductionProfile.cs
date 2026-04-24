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
            // Create: DTO → flat ProductionPackEntry entity
            CreateMap<CreateProductionDto, ProductionPackEntry>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PackNo, opt => opt.Ignore())
                .ForMember(dest => dest.UnitId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Update: DTO → flat ProductionPackEntry entity
            CreateMap<UpdateProductionDto, ProductionPackEntry>()
                .ForMember(dest => dest.PackNo, opt => opt.Ignore())
                .ForMember(dest => dest.UnitId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            // Autocomplete
            CreateMap<ProductionLookupDto, ProductionLookupDto>();
        }
    }
}
