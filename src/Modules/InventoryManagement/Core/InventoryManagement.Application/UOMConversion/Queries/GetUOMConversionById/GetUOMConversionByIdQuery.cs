using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using MediatR;

namespace InventoryManagement.Application.UOMConversion.Queries.GetUOMConversionById
{
    public class GetUOMConversionByIdQuery :IRequest<UOMConversionDto>
    {
        public int Id { get; set; }    
        
    }
}