using BudgetManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace BudgetManagement.Application.MiscMaster.Command.CreateMiscMaster
{
    public class CreateMiscMasterCommand : IRequest<GetMiscMasterDto>
    {

        public int MiscTypeId { get; set; }  
        public string? Code { get; set;}
        public string? Description { get; set;}
        
    }
}