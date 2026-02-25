using AutoMapper;
using SalesManagement.Application.SalesItemPriceMaster.Commands.CreateSalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Commands.UpdateSalesItemPriceMaster;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesItemPriceMasterProfile : Profile
    {
        public SalesItemPriceMasterProfile()
        {
            CreateMap<CreateSalesItemPriceMasterCommand, Domain.Entities.SalesItemPriceMaster>();
            CreateMap<UpdateSalesItemPriceMasterCommand, Domain.Entities.SalesItemPriceMaster>();
        }
    }
}
