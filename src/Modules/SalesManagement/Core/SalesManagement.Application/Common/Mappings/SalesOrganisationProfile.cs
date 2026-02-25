#nullable disable
using AutoMapper;
using SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesOrganisationProfile : Profile
    {
        public SalesOrganisationProfile()
        {
            CreateMap<CreateSalesOrganisationCommand, Domain.Entities.SalesOrganisation>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateSalesOrganisationCommand, Domain.Entities.SalesOrganisation>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
