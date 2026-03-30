using AutoMapper;
using InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.UpdateHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Common.Mappings
{
    public class HSNMasterProfile : Profile
    {

        public HSNMasterProfile()
        {

            CreateMap<InventoryManagement.Domain.Entities.HSNMaster, HSNMasterDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            CreateMap<CreateHSNMasterCommand, InventoryManagement.Domain.Entities.HSNMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateHSNMasterCommand, InventoryManagement.Domain.Entities.HSNMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteHSNMasterCommand, InventoryManagement.Domain.Entities.HSNMaster>();



        }
    }
}