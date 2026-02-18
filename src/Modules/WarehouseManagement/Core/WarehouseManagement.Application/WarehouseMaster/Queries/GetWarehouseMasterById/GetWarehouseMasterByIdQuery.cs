using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster.GetWarehouseMasterById
{
    public class GetWarehouseMasterByIdQuery : IRequest<ApiResponseDTO<WarehouseMasterDto>>
    {
        public int Id { get; set; }
        
    }
}