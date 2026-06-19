using AutoMapper;
using FinanceManagement.Application.VoucherType.Commands.CreateVoucherType;
using FinanceManagement.Application.VoucherType.Commands.UpdateVoucherType;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class VoucherTypeMasterProfile : Profile
    {
        public VoucherTypeMasterProfile()
        {
            CreateMap<CreateVoucherTypeCommand, Domain.Entities.VoucherTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.IsSystem, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.AllowedAccountTypes, opt => opt.Ignore())
                .ForMember(dest => dest.NumberSeries, opt => opt.Ignore());

            CreateMap<UpdateVoucherTypeCommand, Domain.Entities.VoucherTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.AllowedAccountTypes, opt => opt.Ignore())
                .ForMember(dest => dest.NumberSeries, opt => opt.Ignore());
        }
    }
}
