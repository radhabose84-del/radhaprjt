using AutoMapper;
using FinanceManagement.Application.GlAccountMaster.Commands.CreateGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.UpdateGlAccountMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class GlAccountMasterProfile : Profile
    {
        public GlAccountMasterProfile()
        {
            CreateMap<CreateGlAccountMasterCommand, Domain.Entities.GlAccountMaster>()
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())     // set server-side from session
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateGlAccountMasterCommand, Domain.Entities.GlAccountMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
