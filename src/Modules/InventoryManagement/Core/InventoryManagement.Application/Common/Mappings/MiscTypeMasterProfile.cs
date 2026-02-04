using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Common.Mappings
{
    public class MiscTypeMasterProfile  : Profile
    {
        public MiscTypeMasterProfile()
        {
            CreateMap<InventoryManagement.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterDto>();
            
            CreateMap<InventoryManagement.Domain.Entities.MiscTypeMaster, GetMiscTypeMasterAutocompleteDto>();

            CreateMap<CreateMiscTypeMasterCommand, InventoryManagement.Domain.Entities.MiscTypeMaster>()
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
              .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscTypeMasterCommand, InventoryManagement.Domain.Entities.MiscTypeMaster>()
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));
               
            CreateMap<DeleteMiscTypeMasterCommand, InventoryManagement.Domain.Entities.MiscTypeMaster>()
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }

    }
}