using MediatR;

namespace InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion
{
    public class DeleteUOMConversionCommand : IRequest<bool>
    {
          public int Id { get; set; }
        
    }
}