using AutoMapper;
using FinanceManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class TransactionTypeMasterProfile : Profile
    {
        public TransactionTypeMasterProfile()
        {
            CreateMap<CreateTransactionTypeMasterCommand, Domain.Entities.TransactionTypeMaster>()
                .ForMember(dest => dest.UnitId,    opt => opt.Ignore())
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.IsGate,    opt => opt.MapFrom(src => src.IsGate == 1));

            CreateMap<UpdateTransactionTypeMasterCommand, Domain.Entities.TransactionTypeMaster>()
                .ForMember(dest => dest.UnitId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.IsGate, opt => opt.MapFrom(src => src.IsGate == 1));
        }
    }
}
