using AutoMapper;
using QCManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Application.Common.Mappings
{
    public class MiscTypeMasterProfile : Profile
    {
        public MiscTypeMasterProfile()
        {
            CreateMap<CreateMiscTypeMasterCommand, Domain.Entities.MiscTypeMaster>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscTypeMasterCommand, Domain.Entities.MiscTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
