using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace InventoryManagement.Application.MRS.Queries.GetParentWarehouse
{
    public class GetParentWarehouseQuery : IRequest<GetParentWarehouseDto>
    {
         public int WarehouseId { get; set; }
    }
}