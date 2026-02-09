
namespace BudgetManagement.Application.BudgetRequest.Commands.Create
{
    public class BudgetRequestWorkFlowDto
    {
        public int Id { get; set; }
        public string? RequestCode { get; set; }        
        public int StatusId { get; set; }
        public int UnitId { get; set; } 
    }
}