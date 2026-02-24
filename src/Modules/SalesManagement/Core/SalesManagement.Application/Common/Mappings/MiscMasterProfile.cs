#nullable disable
using AutoMapper;
using SalesManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using SalesManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class MiscMasterProfile : Profile
    {
        public MiscMasterProfile()
        {
            CreateMap<CreateMiscMasterCommand, Domain.Entities.MiscMaster>();

            CreateMap<UpdateMiscMasterCommand, Domain.Entities.MiscMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
