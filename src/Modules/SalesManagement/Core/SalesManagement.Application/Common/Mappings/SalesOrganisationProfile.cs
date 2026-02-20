#nullable disable
using AutoMapper;
using SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesOrganisationProfile : Profile
    {
        public SalesOrganisationProfile()
        {
            CreateMap<CreateSalesOrganisationCommand, Domain.Entities.SalesOrganisation>();
            CreateMap<UpdateSalesOrganisationCommand, Domain.Entities.SalesOrganisation>();
        }
    }
}
