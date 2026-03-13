using AutoMapper;
using FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.UpdateEWaybillHeader;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class EWaybillHeaderProfile : Profile
    {
        public EWaybillHeaderProfile()
        {
            CreateMap<CreateEWaybillHeaderCommand, Domain.Entities.EWaybillHeader>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateEWaybillHeaderCommand, Domain.Entities.EWaybillHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
