using AutoMapper;
using Contracts.Dtos.Lookups.Sales;
using SalesManagement.Application.CommissionSplit.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class CommissionSplitProfile : Profile
    {
        public CommissionSplitProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.CommissionSplit, CommissionSplitDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted))
                .ForMember(dest => dest.Details, opt => opt.Ignore());

            // Entity to Lookup DTO
            CreateMap<Domain.Entities.CommissionSplit, CommissionSplitLookupDto>();

            // Command to Entity — children are mapped manually in handler
            CreateMap<CommissionSplit.Commands.CreateCommissionSplit.CreateCommissionSplitCommand, Domain.Entities.CommissionSplit>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.CommissionSplitDetails, opt => opt.Ignore());

            CreateMap<CommissionSplit.Commands.UpdateCommissionSplit.UpdateCommissionSplitCommand, Domain.Entities.CommissionSplit>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.CommissionSplitDetails, opt => opt.Ignore());
        }
    }
}
