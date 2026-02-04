using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.HttpResponse;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using MediatR;

namespace InventoryManagement.Application.UOM.Command.DeleteUOM
{
    public class DeleteUOMCommand  : IRequest<ApiResponseDTO<UOMDto>>
    {
        public int Id { get; set; }
        
        
    }
}