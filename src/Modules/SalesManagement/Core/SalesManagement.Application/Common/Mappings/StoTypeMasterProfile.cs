using AutoMapper;
using SalesManagement.Application.StoTypeMaster.Commands.CreateStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.UpdateStoTypeMaster;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class StoTypeMasterProfile : Profile
    {
        public StoTypeMasterProfile()
        {
            CreateMap<CreateStoTypeMasterCommand, Domain.Entities.StoTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateStoTypeMasterCommand, Domain.Entities.StoTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
