using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace PurchaseManagement.Application.MiscMaster.Command.CreateMiscMaster
{
    public class CreateMiscMasterCommand : IRequest<GetMiscMasterDto>
    {

        public int MiscTypeId { get; set; }  
        public string? Code { get; set;}
        public string? Description { get; set;}
        
    }
}