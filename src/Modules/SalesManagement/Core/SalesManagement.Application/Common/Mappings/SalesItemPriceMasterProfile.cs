#nullable disable
using AutoMapper;
using SalesManagement.Application.SalesItemPriceMaster.Commands.CreateSalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Commands.UpdateSalesItemPriceMaster;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesItemPriceMasterProfile : Profile
    {
        public SalesItemPriceMasterProfile()
        {
            CreateMap<CreateSalesItemPriceMasterCommand, Domain.Entities.SalesItemPriceMaster>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateSalesItemPriceMasterCommand, Domain.Entities.SalesItemPriceMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
