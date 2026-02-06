using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using WarehouseManagement.Application.BinMaster.Command.CreateBinMaster;
using WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster;
using WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster;

namespace WarehouseManagement.Application.Common.Mappings
{
    public class BinMasterProfile : Profile
    {

        public BinMasterProfile()
        {
            CreateMap<WarehouseManagement.Domain.Entities.BinMaster, BinMasterDto>();

            CreateMap<CreateBinMasterCommand, WarehouseManagement.Domain.Entities.BinMaster>();

            CreateMap<UpdateBinMasterCommand, WarehouseManagement.Domain.Entities.BinMaster>()
            .ForMember(d => d.IsActive,     opt => opt.MapFrom(s => s.IsActive == 1
            ? WarehouseManagement.Domain.Common.BaseEntity.Status.Active : WarehouseManagement.Domain.Common.BaseEntity.Status.Inactive));
            
           
        }
        
    }
}