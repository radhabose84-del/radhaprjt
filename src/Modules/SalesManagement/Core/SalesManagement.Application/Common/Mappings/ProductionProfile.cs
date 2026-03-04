using AutoMapper;
using SalesManagement.Application.ProductionPack.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class ProductionProfile : Profile
    {
        public ProductionProfile()
        {
            // Create: DTO → Header entity (with nested details)
            CreateMap<CreateProductionDto, ProductionPackHeader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PackNo, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionPackDetails, opt => opt.MapFrom(src => src.ProductionPackDetails))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Create: Detail DTO → Detail entity
            CreateMap<CreateProductionPackDetailDto, ProductionPackDetail>();

            // Update: DTO → Header entity
            CreateMap<UpdateProductionDto, ProductionPackHeader>()
                .ForMember(dest => dest.PackNo, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionPackDetails, opt => opt.MapFrom(src => src.ProductionPackDetails))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            // Update: Detail DTO → Detail entity
            CreateMap<UpdateProductionPackDetailDto, ProductionPackDetail>();

            // Autocomplete: LookupDto → LookupDto
            CreateMap<ProductionLookupDto, ProductionLookupDto>();
        }
    }
}
