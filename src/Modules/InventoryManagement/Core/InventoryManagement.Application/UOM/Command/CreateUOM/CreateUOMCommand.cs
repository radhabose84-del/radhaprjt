using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.HttpResponse;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using MediatR;

namespace InventoryManagement.Application.UOM.Command.CreateUOM
{
    public class CreateUOMCommand  : IRequest<ApiResponseDTO<UOMDto>>
    {
        public string? Code { get; set; }
        public string? UOMName { get; set; }
        public int SortOrder { get; set; }
        public int UOMTypeId { get; set; }
        
    }
}