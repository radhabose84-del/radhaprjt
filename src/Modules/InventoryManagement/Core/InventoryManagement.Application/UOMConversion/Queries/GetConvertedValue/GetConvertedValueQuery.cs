using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.UOMConversion.Queries.GetConvertedValue
{
    public class GetConvertedValueQuery : IRequest<ApiResponseDTO<decimal>>
    {
        public int FromUOMId { get; set; }
        public int ToUOMId { get; set; }
        public decimal Quantity { get; set; }
    
        
    }
}