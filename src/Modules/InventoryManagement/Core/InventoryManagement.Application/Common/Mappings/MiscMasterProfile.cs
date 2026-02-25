using AutoMapper;
using InventoryManagement.Application.MiscMaster.Command.CreateMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Common.Mappings
{
    public class MiscMasterProfile  : Profile


    {
        public MiscMasterProfile()
        {
            CreateMap<InventoryManagement.Domain.Entities.MiscMaster, GetMiscMasterDto>();

            CreateMap<InventoryManagement.Domain.Entities.MiscMaster, GetMiscMasterAutoCompleteDto>();

            CreateMap<CreateMiscMasterCommand, InventoryManagement.Domain.Entities.MiscMaster>()
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, InventoryManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteMiscMasterCommand, InventoryManagement.Domain.Entities.MiscMaster>()
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}