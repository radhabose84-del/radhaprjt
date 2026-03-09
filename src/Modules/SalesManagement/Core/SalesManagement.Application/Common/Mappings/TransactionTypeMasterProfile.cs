using AutoMapper;
using SalesManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster;
using SalesManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class TransactionTypeMasterProfile : Profile
    {
        public TransactionTypeMasterProfile()
        {
            CreateMap<CreateTransactionTypeMasterCommand, Domain.Entities.TransactionTypeMaster>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateTransactionTypeMasterCommand, Domain.Entities.TransactionTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
