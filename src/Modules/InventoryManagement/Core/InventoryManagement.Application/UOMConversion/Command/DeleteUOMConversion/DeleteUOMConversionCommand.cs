using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion
{
    public class DeleteUOMConversionCommand : IRequest<bool>
    {
          public int Id { get; set; }
        
    }
}