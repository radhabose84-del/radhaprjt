using AutoMapper;
using InventoryManagement.Application.ProcurementType.Commands.CreateProcurementType;
using InventoryManagement.Application.ProcurementType.Commands.UpdateProcurementType;
using InventoryManagement.Application.ProcurementType.Commands.DeleteProcurementType;
using InventoryManagement.Application.ProcurementType.Dto;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Common.Mappings
{
    public class ProcurementTypeProfile : Profile
    {
        public ProcurementTypeProfile()
        {
            CreateMap<InventoryManagement.Domain.Entities.ProcurementType, ProcurementTypeDto>();

            CreateMap<InventoryManagement.Domain.Entities.ProcurementType, ProcurementTypeLookupDto>();

            CreateMap<CreateProcurementTypeCommand, InventoryManagement.Domain.Entities.ProcurementType>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateProcurementTypeCommand, InventoryManagement.Domain.Entities.ProcurementType>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteProcurementTypeCommand, InventoryManagement.Domain.Entities.ProcurementType>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}
