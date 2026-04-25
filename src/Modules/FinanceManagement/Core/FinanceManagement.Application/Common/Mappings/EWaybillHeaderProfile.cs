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
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.EWaybillDetails, opt => opt.MapFrom(src => src.Details));

            CreateMap<CreateEWaybillDetailDto, Domain.Entities.EWaybillDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EWaybillHeaderId, opt => opt.Ignore())
                .ForMember(dest => dest.EWaybillHeader,  opt => opt.Ignore())
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            CreateMap<UpdateEWaybillHeaderCommand, Domain.Entities.EWaybillHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
