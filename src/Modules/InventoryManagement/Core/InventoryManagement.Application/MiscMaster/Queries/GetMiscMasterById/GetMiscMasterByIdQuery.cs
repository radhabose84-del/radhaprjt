using InventoryManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace InventoryManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery  :  IRequest<GetMiscMasterDto>
    { 
         public int Id { get; set; }
        
    }
}