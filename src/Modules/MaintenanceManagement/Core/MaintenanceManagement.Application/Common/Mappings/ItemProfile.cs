using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Item.ItemGroup.Queries;
using MaintenanceManagement.Application.Item.ItemMaster.Queries;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class ItemProfile : Profile
    {
        public ItemProfile()
        {
            CreateMap<MaintenanceManagement.Domain.Entities.ItemGroupCode, GetItemGroupDto>();
            CreateMap<MaintenanceManagement.Domain.Entities.ItemMaster, GetItemMasterDto>();
          
        }
    }
}