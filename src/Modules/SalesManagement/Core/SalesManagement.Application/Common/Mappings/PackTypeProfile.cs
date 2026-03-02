using AutoMapper;
using SalesManagement.Application.PackType.Commands.CreatePackType;
using SalesManagement.Application.PackType.Commands.UpdatePackType;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class PackTypeProfile : Profile
    {
        public PackTypeProfile()
        {
            CreateMap<CreatePackTypeCommand, Domain.Entities.PackType>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.GrossWeight, opt => opt.MapFrom(src => src.NetWeight + src.TareWeight));

            CreateMap<UpdatePackTypeCommand, Domain.Entities.PackType>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.GrossWeight, opt => opt.MapFrom(src => src.NetWeight + src.TareWeight));
        }
    }
}
