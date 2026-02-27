using AutoMapper;
using SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.UpdateItemPriceMaster;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class ItemPriceMasterProfile : Profile
    {
        public ItemPriceMasterProfile()
        {
            CreateMap<CreateItemPriceMasterCommand, Domain.Entities.ItemPriceMaster>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateItemPriceMasterCommand, Domain.Entities.ItemPriceMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
