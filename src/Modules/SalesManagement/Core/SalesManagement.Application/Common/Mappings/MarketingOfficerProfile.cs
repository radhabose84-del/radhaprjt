using AutoMapper;
using SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.UpdateMarketingOfficer;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class MarketingOfficerProfile : Profile
    {
        public MarketingOfficerProfile()
        {
            CreateMap<CreateMarketingOfficerCommand, Domain.Entities.MarketingOfficer>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.OfficerSalesGroups, opt => opt.Ignore());

            CreateMap<UpdateMarketingOfficerCommand, Domain.Entities.MarketingOfficer>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.OfficerSalesGroups, opt => opt.Ignore());
        }
    }
}
