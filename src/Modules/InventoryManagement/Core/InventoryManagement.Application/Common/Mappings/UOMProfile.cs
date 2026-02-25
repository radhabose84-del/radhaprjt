using AutoMapper;
using InventoryManagement.Application.UOM.Command.CreateUOM;
using InventoryManagement.Application.UOM.Command.DeleteUOM;
using InventoryManagement.Application.UOM.Command.UpdateUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Common.Mappings
{
    public class UOMProfile  : Profile
    {
        public UOMProfile()
        {
            CreateMap<CreateUOMCommand, InventoryManagement.Domain.Entities.UOM>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateUOMCommand, InventoryManagement.Domain.Entities.UOM>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteUOMCommand, InventoryManagement.Domain.Entities.UOM>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
            CreateMap<Domain.Entities.UOM, UOMDto>();
            CreateMap<Domain.Entities.UOM, UOMAutoCompleteDto>();

        }
        
    }
}