using AutoMapper;
using InventoryManagement.Application.UsageType.Commands.CreateUsageType;
using InventoryManagement.Application.UsageType.Commands.UpdateUsageType;
using InventoryManagement.Application.UsageType.Commands.DeleteUsageType;
using InventoryManagement.Application.UsageType.Dto;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Common.Mappings
{
    public class UsageTypeProfile : Profile
    {
        public UsageTypeProfile()
        {
            CreateMap<InventoryManagement.Domain.Entities.UsageType, UsageTypeDto>();

            CreateMap<InventoryManagement.Domain.Entities.UsageType, UsageTypeLookupDto>();

            CreateMap<CreateUsageTypeCommand, InventoryManagement.Domain.Entities.UsageType>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateUsageTypeCommand, InventoryManagement.Domain.Entities.UsageType>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteUsageTypeCommand, InventoryManagement.Domain.Entities.UsageType>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}
