using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.UpdateHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;

namespace InventoryManagement.Application.Common.Mappings
{
    public class HSNMasterProfile : Profile
    {

        public HSNMasterProfile()
        {

            CreateMap<InventoryManagement.Domain.Entities.HSNMaster, HSNMasterDto>();

            CreateMap<CreateHSNMasterCommand, InventoryManagement.Domain.Entities.HSNMaster>();

            CreateMap<UpdateHSNMasterCommand, InventoryManagement.Domain.Entities.HSNMaster>();

            CreateMap<DeleteHSNMasterCommand, InventoryManagement.Domain.Entities.HSNMaster>();



        }
    }
}