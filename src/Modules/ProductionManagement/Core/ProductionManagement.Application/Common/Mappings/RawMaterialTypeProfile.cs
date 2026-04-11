using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.RawMaterialType.Commands.CreateRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.UpdateRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class RawMaterialTypeProfile : Profile
    {
        public RawMaterialTypeProfile()
        {
            // Create command → Entity (defaults via ForMember per CLAUDE.md Rule #15)
            CreateMap<CreateRawMaterialTypeCommand, Domain.Entities.RawMaterialType>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Update command → Entity (IsActive mapped from int 0/1; Code is excluded — immutable)
            CreateMap<UpdateRawMaterialTypeCommand, Domain.Entities.RawMaterialType>()
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.RawMaterialType, RawMaterialTypeDto>();
            CreateMap<RawMaterialTypeDto, RawMaterialTypeDto>();
            CreateMap<Domain.Entities.RawMaterialType, RawMaterialTypeLookupDto>();
            CreateMap<RawMaterialTypeLookupDto, RawMaterialTypeLookupDto>();
        }
    }
}
