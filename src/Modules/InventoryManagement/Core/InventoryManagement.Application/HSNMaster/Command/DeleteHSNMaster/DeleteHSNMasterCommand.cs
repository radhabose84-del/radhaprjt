using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster
{
    public class DeleteHSNMasterCommand: IRequest<ApiResponseDTO<bool>>
    {
        public int Id { get; set; }
        
    }
}