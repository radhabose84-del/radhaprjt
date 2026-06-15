using AutoMapper;
using FinanceManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using FinanceManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class MiscMasterProfile : Profile
    {
        public MiscMasterProfile()
        {
            CreateMap<CreateMiscMasterCommand, Domain.Entities.MiscMaster>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, Domain.Entities.MiscMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
