using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterById
{
    public class GetHSNMasterByIdQuery : IRequest<ApiResponseDTO<HSNMasterDto>>
    {
        public int Id { get; set; }
    }
}