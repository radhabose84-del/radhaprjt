using AutoMapper;
using FinanceManagement.Application.AccountGroup.Commands.CreateAccountGroup;
using FinanceManagement.Application.AccountGroup.Commands.UpdateAccountGroup;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class AccountGroupProfile : Profile
    {
        public AccountGroupProfile()
        {
            // Level is derived in the command repository from the parent — not mapped here.
            CreateMap<CreateAccountGroupCommand, Domain.Entities.AccountGroup>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // GroupCode is immutable on update; only GroupName + IsActive change.
            CreateMap<UpdateAccountGroupCommand, Domain.Entities.AccountGroup>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
