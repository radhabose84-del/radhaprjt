using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.UOM.Queries.GetUOMTypeAutoComplete
{
    public class GetUOMTypeAutoCompleteQuery  : IRequest<ApiResponseDTO<List<UOMTypeAutoCompleteDto>>>
    {
        public string? SearchPattern { get; set; }
        
    }
}