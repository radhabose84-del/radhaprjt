using AutoMapper;
using SalesManagement.Application.MovementTypeConfig.Commands.CreateMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.UpdateMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class MovementTypeConfigProfile : Profile
    {
        public MovementTypeConfigProfile()
        {
            // Entity → DTO
            CreateMap<Domain.Entities.MovementTypeConfig, MovementTypeConfigDto>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => src.IsActive  == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            CreateMap<Domain.Entities.MovementTypeConfig, MovementTypeConfigLookupDto>();

            // Create Command → Entity
            CreateMap<CreateMovementTypeConfigCommand, Domain.Entities.MovementTypeConfig>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Update Command → Entity
            CreateMap<UpdateMovementTypeConfigCommand, Domain.Entities.MovementTypeConfig>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
