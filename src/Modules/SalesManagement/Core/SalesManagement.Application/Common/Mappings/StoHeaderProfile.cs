using AutoMapper;
using SalesManagement.Application.StoHeader.Commands.CreateStoHeader;
using SalesManagement.Application.StoHeader.Commands.UpdateStoHeader;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class StoHeaderProfile : Profile
    {
        public StoHeaderProfile()
        {
            // Create: Command → Entity (header + details)
            CreateMap<CreateStoHeaderCommand, Domain.Entities.StoHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<CreateStoDetailDto, Domain.Entities.StoDetail>();

            // Update: Command → Entity (header only, details handled separately)
            CreateMap<UpdateStoHeaderCommand, Domain.Entities.StoHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<UpdateStoDetailDto, Domain.Entities.StoDetail>();
        }
    }
}
