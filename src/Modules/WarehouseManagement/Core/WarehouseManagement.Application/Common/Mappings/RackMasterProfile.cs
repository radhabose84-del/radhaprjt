using AutoMapper;
using WarehouseManagement.Application.RackMaster.Command.CreateRackMaster;
using WarehouseManagement.Application.RackMaster.Command.UpdateRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;

namespace WarehouseManagement.Application.Common.Mappings
{
    public class RackMasterProfile : Profile
    {

        public RackMasterProfile()
        {
            CreateMap<WarehouseManagement.Domain.Entities.RackMaster, RackMasterDto>();

            CreateMap<CreateRackMasterCommand, WarehouseManagement.Domain.Entities.RackMaster>();

            CreateMap<UpdateRackMasterCommand, WarehouseManagement.Domain.Entities.RackMaster>()
            .ForMember(d => d.IsActive,     opt => opt.MapFrom(s => s.IsActive == 1
            ? WarehouseManagement.Domain.Common.BaseEntity.Status.Active : WarehouseManagement.Domain.Common.BaseEntity.Status.Inactive))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            
          //  CreateMap<UpdateRackMasterCommand, WarehouseManagement.Domain.Entities.RackMaster>();

            // CreateMap<DeleteRackMasterCommand, WarehouseManagement.Domain.Entities.RackMaster>();
        }
        
    }
}