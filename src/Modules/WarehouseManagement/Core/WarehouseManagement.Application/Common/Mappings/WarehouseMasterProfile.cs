using AutoMapper;
using WarehouseManagement.Application.WarehouseMaster.Command.CreateWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.UpdateWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.Application.Common.Mappings
{
    public class WarehouseMasterProfile : Profile
    {

        public WarehouseMasterProfile()
        {
            CreateMap<WarehouseManagement.Domain.Entities.WarehouseMaster, WarehouseMasterDto>();

            CreateMap<CreateWarehouseMasterCommand, WarehouseManagement.Domain.Entities.WarehouseMaster>()
                .ForMember(dest => dest.AllowedItemGroups, opt => opt.Ignore());

            CreateMap<UpdateWarehouseMasterCommand, WarehouseManagement.Domain.Entities.WarehouseMaster>()
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.Id, opt => opt.Ignore());    
        }
    }
}