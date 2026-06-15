using AutoMapper;
using FinanceManagement.Application.AccountTypeMaster.Commands.CreateAccountTypeMaster;
using FinanceManagement.Application.AccountTypeMaster.Commands.UpdateAccountTypeMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class AccountTypeMasterProfile : Profile
    {
        public AccountTypeMasterProfile()
        {
            CreateMap<CreateAccountTypeMasterCommand, Domain.Entities.AccountTypeMaster>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateAccountTypeMasterCommand, Domain.Entities.AccountTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
