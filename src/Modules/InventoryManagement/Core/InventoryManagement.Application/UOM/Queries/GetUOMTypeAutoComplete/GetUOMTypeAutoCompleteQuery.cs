using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.HttpResponse;
using MediatR;

namespace InventoryManagement.Application.UOM.Queries.GetUOMTypeAutoComplete
{
    public class GetUOMTypeAutoCompleteQuery  : IRequest<ApiResponseDTO<List<UOMTypeAutoCompleteDto>>>
    {
        public string? SearchPattern { get; set; }
        
    }
}