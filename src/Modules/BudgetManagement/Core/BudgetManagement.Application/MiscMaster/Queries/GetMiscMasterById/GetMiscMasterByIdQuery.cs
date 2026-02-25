using BudgetManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace BudgetManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery :  IRequest<GetMiscMasterDto>
    { 
         public int Id { get; set; }
        
    }
}