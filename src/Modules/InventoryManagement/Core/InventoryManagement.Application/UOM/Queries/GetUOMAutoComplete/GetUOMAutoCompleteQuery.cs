using Contracts.Common;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using MediatR;

namespace InventoryManagement.Application.UOM.Queries.GetUOMAutoComplete
{
    public class GetUOMAutoCompleteQuery   : IRequest<ApiResponseDTO<List<UOMAutoCompleteDto>>>
    {
        public string? SearchPattern { get; set; }
        
    }
}