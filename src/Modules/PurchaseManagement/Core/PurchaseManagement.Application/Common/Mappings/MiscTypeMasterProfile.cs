using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class MiscTypeMasterProfile : Profile
    {
        public MiscTypeMasterProfile()
        {
            CreateMap<PurchaseManagement.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterDto>();
            
            CreateMap<PurchaseManagement.Domain.Entities.MiscTypeMaster, GetMiscTypeMasterAutocompleteDto>();

            CreateMap<CreateMiscTypeMasterCommand, PurchaseManagement.Domain.Entities.MiscTypeMaster>()
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
              .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscTypeMasterCommand, PurchaseManagement.Domain.Entities.MiscTypeMaster>()
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));
               
            CreateMap<DeleteMiscTypeMasterCommand, PurchaseManagement.Domain.Entities.MiscTypeMaster>()
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }

    }
}